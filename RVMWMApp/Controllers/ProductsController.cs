using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RVMWMApp.Data;
using RVMWMApp.Models;

namespace RVMWMApp.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ---------------------------------------------------------------
        // PUBLIC: browse products
        // ---------------------------------------------------------------
        public async Task<IActionResult> Index(string? search = null, string? category = null,
            decimal? minPrice = null, decimal? maxPrice = null, bool inStockOnly = false,
            bool featuredOnly = false, bool onSaleOnly = false, int minimumRating = 0)
        {
            var products = _context.Products.AsQueryable();

            // TEMPORARY: Search without Tags to avoid the error
            if (!string.IsNullOrEmpty(search))
            {
                products = products.Where(p =>
                    (p.Name != null && p.Name.Contains(search)) ||
                    (p.Description != null && p.Description.Contains(search))
                );
            }

            if (!string.IsNullOrEmpty(category) && category != "All Categories")
            {
                products = products.Where(p => p.Category == category);
            }

            if (inStockOnly)
            {
                products = products.Where(p => p.Stock > 0);
            }

            if (featuredOnly)
            {
                products = products.Where(p => p.IsFeatured);
            }

            if (onSaleOnly)
            {
                products = products.Where(p => p.IsOnSale);
            }

            // Only filter on price/rating when the user actually set a value,
            // so expensive or unrated products aren't hidden by default.
            if (minPrice.HasValue)
            {
                products = products.Where(p => p.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                products = products.Where(p => p.Price <= maxPrice.Value);
            }

            if (minimumRating > 0)
            {
                products = products.Where(p => p.Rating >= minimumRating);
            }

            var productList = await products.ToListAsync();

            ViewBag.Categories = await _context.Products
                .Where(p => p.Category != null)
                .Select(p => p.Category!)
                .Distinct()
                .ToListAsync();

            ViewBag.SearchTerm = search;
            ViewBag.SelectedCategory = category;

            return View(productList);
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // ---------------------------------------------------------------
        // ADMIN ONLY: create / edit / delete
        // ---------------------------------------------------------------

        // GET: Products/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Products/Create
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description,Price,Stock,Category,ImagePath,ImageUrl,Rating,ReviewCount,IsFeatured,IsOnSale")] Product product)
        {
            if (ModelState.IsValid)
            {
                _context.Add(product);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Product created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }

        // GET: Products/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        // POST: Products/Edit/5
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            // Only fields that are actually on the form get updated, so anything
            // missing from the form (e.g. ImageUrl) keeps its saved value.
            if (await TryUpdateModelAsync(product, "",
                    p => p.Name, p => p.Description, p => p.Price, p => p.Stock,
                    p => p.Category, p => p.ImagePath, p => p.ImageUrl, p => p.Rating,
                    p => p.ReviewCount, p => p.IsFeatured, p => p.IsOnSale))
            {
                try
                {
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Product updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(id))
                    {
                        return NotFound();
                    }
                    throw;
                }
            }
            return View(product);
        }

        // GET: Products/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Products/Delete/5
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                TempData["Error"] = "Product not found.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Product deleted successfully!";
            }
            catch (DbUpdateException)
            {
                TempData["Error"] = "This product can't be deleted because it is linked to existing orders.";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
}