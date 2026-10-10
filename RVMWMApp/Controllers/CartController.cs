using System.Globalization;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RVMWMApp.Data;
using RVMWMApp.Models;

namespace RVMWMApp.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;
        private readonly ILogger<CartController> _logger;
        private const string CartSessionKey = "CartSession";

        // Order statuses used by the payment flow
        private const string AwaitingPayment = "Awaiting Payment";
        private const string Paid = "Paid";

        public CartController(ApplicationDbContext context,
                              IConfiguration config,
                              ILogger<CartController> logger)
        {
            _context = context;
            _config = config;
            _logger = logger;
        }

        // ---------------------------------------------------------------
        // CART
        // ---------------------------------------------------------------
        public IActionResult Index()
        {
            var cart = GetCart();
            return View(cart);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(int id, int quantity = 1)
        {
            if (quantity < 1) quantity = 1;

            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
            if (product == null) return NotFound();

            var cart = GetCart();
            var existingItem = cart.FirstOrDefault(i => i.ProductId == id);
            var newQuantity = (existingItem?.Quantity ?? 0) + quantity;

            if (newQuantity > product.Stock)
            {
                TempData["Error"] = $"Only {product.Stock} of {product.Name} in stock.";
                return RedirectToAction("Index", "Products");
            }

            if (existingItem != null)
            {
                existingItem.Quantity = newQuantity;
            }
            else
            {
                cart.Add(new CartItem
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Price = product.Price,
                    Quantity = quantity,
                    ImageUrl = product.ImageUrl ?? product.ImagePath
                });
            }

            SaveCart(cart);
            TempData["Success"] = $"{product.Name} added to cart!";
            return RedirectToAction("Index", "Products");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveFromCart(int id)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(i => i.ProductId == id);
            if (item != null)
                cart.Remove(item);

            SaveCart(cart);
            return RedirectToAction("Index");
        }

        [Authorize]
        public IActionResult Checkout()
        {
            var cart = GetCart();
            if (!cart.Any())
            {
                TempData["Error"] = "Your cart is empty!";
                return RedirectToAction("Index");
            }

            return View(cart);
        }

        // ---------------------------------------------------------------
        // PAYFAST
        // 1. ProcessPayment saves an Order ("Awaiting Payment") and sends the user to PayFast.
        // 2. PayFast calls PaymentNotify (server to server); only then is the order marked Paid
        //    and stock reduced.
        // 3. PaymentSuccess / PaymentCancel are just where the user's browser lands.
        // ---------------------------------------------------------------
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessPayment()
        {
            var cart = GetCart();
            if (!cart.Any()) return RedirectToAction("Index");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Challenge();

            // Re-read prices and stock from the database so the cart can't be out of date
            var ids = cart.Select(i => i.ProductId).ToList();
            var products = await _context.Products
                .Where(p => ids.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id);

            var problems = new List<string>();
            foreach (var item in cart.ToList())
            {
                if (!products.TryGetValue(item.ProductId, out var product))
                {
                    cart.Remove(item);
                    problems.Add($"{item.ProductName} is no longer available and was removed.");
                }
                else if (product.Stock < item.Quantity)
                {
                    problems.Add($"Only {product.Stock} of {product.Name} in stock. Please update your cart.");
                }
                else
                {
                    item.Price = product.Price;
                }
            }

            if (problems.Any())
            {
                SaveCart(cart);
                TempData["Error"] = string.Join(" ", problems);
                return RedirectToAction("Index");
            }

            SaveCart(cart); // saves the refreshed prices
            var totalAmount = cart.Sum(i => i.Price * i.Quantity);

            // Save the order first so PayFast's notification can find it
            var order = new Order
            {
                UserId = userId,
                OrderDate = DateTime.Now,
                TotalAmount = totalAmount,
                Status = AwaitingPayment,
                WasteType = "Product purchase",
                CollectionAddress = "N/A",
                EstimatedWeight = 0,
                Items = cart.Select(i => new OrderItem
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    Price = i.Price
                }).ToList()
            };
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            var merchantId = _config["PayFast:MerchantId"] ?? "10000100";
            var merchantKey = _config["PayFast:MerchantKey"] ?? "46f0cd694581a";
            var passphrase = _config["PayFast:Passphrase"] ?? string.Empty;
            var processUrl = _config["PayFast:ProcessUrl"] ?? "https://sandbox.payfast.co.za/eng/process";

            // Field ORDER matters for the signature (PayFast's documented order)
            var fields = new List<KeyValuePair<string, string>>
            {
                new("merchant_id", merchantId),
                new("merchant_key", merchantKey),
                new("return_url", AbsoluteUrl("PaymentSuccess") + $"?orderId={order.Id}"),
                new("cancel_url", AbsoluteUrl("PaymentCancel") + $"?orderId={order.Id}"),
                new("notify_url", AbsoluteUrl("PaymentNotify")),
                new("email_address", User.FindFirstValue(ClaimTypes.Email) ?? string.Empty),
                new("m_payment_id", order.Id.ToString()),
                new("amount", totalAmount.ToString("F2", CultureInfo.InvariantCulture)),
                new("item_name", $"Recyclables order #{order.Id}")
            };

            var signature = Sign(fields, passphrase);

            var query = string.Join("&", fields
                .Where(kv => !string.IsNullOrEmpty(kv.Value))
                .Select(kv => $"{kv.Key}={PfEncode(kv.Value)}"));

            return Redirect($"{processUrl}?{query}&signature={signature}");
        }

        // Browser return. Payment is confirmed by PaymentNotify, not here.
        public IActionResult PaymentSuccess(int? orderId)
        {
            HttpContext.Session.Remove(CartSessionKey);
            ViewBag.Message = "🎉 Payment received! Thank you for purchasing recyclables. Your order will show as Paid once PayFast confirms it.";
            return View("PaymentResult");
        }

        public async Task<IActionResult> PaymentCancel(int? orderId)
        {
            // Cart is kept so the user can try again; the unpaid order is cancelled
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (orderId != null && userId != null)
            {
                var order = await _context.Orders.FirstOrDefaultAsync(o =>
                    o.Id == orderId && o.UserId == userId && o.Status == AwaitingPayment);
                if (order != null)
                {
                    order.Status = "Cancelled";
                    await _context.SaveChangesAsync();
                }
            }

            ViewBag.Message = "⚠️ Payment Cancelled. Your recyclables were not purchased.";
            return View("PaymentResult");
        }

        // PayFast calls this server-to-server (POST), so no session or login here.
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> PaymentNotify()
        {
            if (!Request.HasFormContentType) return BadRequest();

            var passphrase = _config["PayFast:Passphrase"] ?? string.Empty;

            var fields = Request.Form
                .Where(kv => kv.Key != "signature")
                .Select(kv => new KeyValuePair<string, string>(kv.Key, kv.Value.ToString()))
                .ToList();

            var postedSignature = Request.Form["signature"].ToString();
            var expectedSignature = Sign(fields, passphrase);

            if (!string.Equals(postedSignature, expectedSignature, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("PayFast notify rejected: signature mismatch");
                return BadRequest();
            }

            var paymentId = Request.Form["m_payment_id"].ToString();
            var paymentStatus = Request.Form["payment_status"].ToString();
            var amountGross = Request.Form["amount_gross"].ToString();

            if (!int.TryParse(paymentId, out var orderId))
            {
                _logger.LogWarning("PayFast notify: bad m_payment_id {PaymentId}", paymentId);
                return BadRequest();
            }

            var order = await _context.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                _logger.LogWarning("PayFast notify: order {OrderId} not found", orderId);
                return Ok();
            }

            // Already handled (PayFast can send the same notification more than once)
            if (order.Status != AwaitingPayment) return Ok();

            if (paymentStatus == "COMPLETE")
            {
                // The amount PayFast says was paid must match what we asked for
                if (!decimal.TryParse(amountGross, NumberStyles.Number, CultureInfo.InvariantCulture, out var amountPaid)
                    || amountPaid != order.TotalAmount)
                {
                    _logger.LogWarning("PayFast notify: amount mismatch for order {OrderId} ({Paid} vs {Expected})",
                        orderId, amountGross, order.TotalAmount);
                    return BadRequest();
                }

                order.Status = Paid;

                foreach (var item in order.Items)
                {
                    var product = await _context.Products.FindAsync(item.ProductId);
                    if (product != null)
                    {
                        product.Stock = Math.Max(0, product.Stock - item.Quantity);
                    }
                }
            }
            else if (paymentStatus == "CANCELLED" || paymentStatus == "FAILED")
            {
                order.Status = "Cancelled";
            }

            await _context.SaveChangesAsync();
            return Ok();
        }

        // ---------------------------------------------------------------
        // HELPERS
        // ---------------------------------------------------------------
        private List<CartItem> GetCart()
        {
            var cartJson = HttpContext.Session.GetString(CartSessionKey);
            return string.IsNullOrEmpty(cartJson)
                ? new List<CartItem>()
                : JsonConvert.DeserializeObject<List<CartItem>>(cartJson) ?? new List<CartItem>();
        }

        private void SaveCart(List<CartItem> cart)
        {
            var cartJson = JsonConvert.SerializeObject(cart);
            HttpContext.Session.SetString(CartSessionKey, cartJson);
        }

        // PayFast can't reach localhost. For local testing, run ngrok and put its
        // https address in PayFast:PublicBaseUrl; otherwise the current host is used.
        private string AbsoluteUrl(string action)
        {
            var path = Url.Action(action, "Cart") ?? $"/Cart/{action}";
            var publicBase = _config["PayFast:PublicBaseUrl"]?.TrimEnd('/');
            return !string.IsNullOrEmpty(publicBase)
                ? publicBase + path
                : $"{Request.Scheme}://{Request.Host}{path}";
        }

        // PHP-style url encoding, which is what PayFast signs against
        private static string PfEncode(string value)
        {
            return WebUtility.UrlEncode(value.Trim())
                .Replace("!", "%21")
                .Replace("*", "%2A")
                .Replace("(", "%28")
                .Replace(")", "%29");
        }

        private static string Sign(IEnumerable<KeyValuePair<string, string>> fields, string passphrase)
        {
            var payload = string.Join("&", fields
                .Where(kv => !string.IsNullOrEmpty(kv.Value))
                .Select(kv => $"{kv.Key}={PfEncode(kv.Value)}"));

            if (!string.IsNullOrEmpty(passphrase))
                payload += $"&passphrase={PfEncode(passphrase)}";

            var hash = MD5.HashData(Encoding.UTF8.GetBytes(payload));
            return Convert.ToHexString(hash).ToLowerInvariant();
        }
    }
}