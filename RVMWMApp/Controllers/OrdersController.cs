using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RVMWMApp.Data;
using RVMWMApp.Models;

namespace RVMWMApp.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrdersController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var orders = await _context.Orders
                .Include(o => o.Items)
                .ThenInclude(i => i.Product)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Order order)
        {
            // Server-controlled fields: don't let them block validation
            ModelState.Remove("User");
            ModelState.Remove("UserId");
            ModelState.Remove("Status");
            ModelState.Remove("OrderDate");
            ModelState.Remove("Items");

            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User);
                if (userId == null)
                {
                    ModelState.AddModelError(string.Empty, "Unable to determine user ID.");
                    return View(order);
                }

                // Overwrite anything the browser might have posted for these fields
                order.Id = 0;
                order.UserId = userId;
                order.OrderDate = DateTime.Now;
                order.Status = "Pending";

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Waste collection order created successfully!";
                return RedirectToAction("Index");
            }
            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null || order.UserId != _userManager.GetUserId(User))
            {
                TempData["Error"] = "Order not found.";
                return RedirectToAction("Index");
            }

            // Only orders that haven't been processed yet can be cancelled
            if (order.Status != "Pending")
            {
                TempData["Error"] = $"A {order.Status} order can't be cancelled.";
                return RedirectToAction("Index");
            }

            order.Status = "Cancelled";
            await _context.SaveChangesAsync();
            TempData["Success"] = "Order cancelled successfully.";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin,Operator")]
        public async Task<IActionResult> Manage()
        {
            var orders = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.Items)
                .ThenInclude(i => i.Product)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
            return View(orders);
        }

        [Authorize(Roles = "Admin,Operator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            if (string.IsNullOrWhiteSpace(status) || status.Length > 50)
            {
                TempData["Error"] = "Please choose a valid status.";
                return RedirectToAction("Manage");
            }

            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                TempData["Error"] = "Order not found.";
                return RedirectToAction("Manage");
            }

            order.Status = status;
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Order status updated to {status}";
            return RedirectToAction("Manage");
        }
    }
}