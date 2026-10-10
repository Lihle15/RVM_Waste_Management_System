using System.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RVMWMApp.Data;
using RVMWMApp.Models;

namespace RVMWMApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<AdminController> _logger;

        public AdminController(ApplicationDbContext context,
                               UserManager<ApplicationUser> userManager,
                               ILogger<AdminController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        // ---------------------------------------------------------------
        // DASHBOARD
        // ---------------------------------------------------------------
        public async Task<IActionResult> Dashboard()
        {
            try
            {
                var model = new AdminDashboardViewModel
                {
                    TotalUsers = await _userManager.Users.CountAsync(),
                    TotalProducts = await _context.Products.CountAsync(),
                    TotalRVMs = await _context.RVMs.CountAsync(),
                    ActiveRVMs = await _context.RVMs.CountAsync(r => r.IsActive),
                    LowStockProducts = await _context.Products.CountAsync(p => p.Stock < 10),

                    // Revenue = total of orders PayFast has confirmed as paid
                    TotalRevenue = await _context.Orders
                        .Where(o => o.Status == "Paid")
                        .SumAsync(o => o.TotalAmount),

                    RecentActivity = await GetRecentActivityAsync()
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading admin dashboard");
                TempData["Error"] = "Error loading dashboard data.";
                return View(new AdminDashboardViewModel());
            }
        }

        // ---------------------------------------------------------------
        // USER MANAGEMENT
        // ---------------------------------------------------------------
        public async Task<IActionResult> Users()
        {
            var users = await _userManager.Users.ToListAsync();
            var userRoles = new List<UserWithRolesViewModel>();

            foreach (var user in users)
            {
                if (user == null) continue;
                var roles = await _userManager.GetRolesAsync(user);
                userRoles.Add(new UserWithRolesViewModel
                {
                    User = user,
                    Roles = roles
                });
            }

            return View(userRoles);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateUserRole(string userId, string role, bool isAdded)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    TempData["Error"] = "User not found.";
                    return RedirectToAction("Users");
                }

                // Stop admins locking themselves out
                if (!isAdded && role == "Admin" && user.Id == _userManager.GetUserId(User))
                {
                    TempData["Error"] = "You cannot remove your own Admin role.";
                    return RedirectToAction("Users");
                }

                IdentityResult result = isAdded
                    ? await _userManager.AddToRoleAsync(user, role)
                    : await _userManager.RemoveFromRoleAsync(user, role);

                if (result.Succeeded)
                {
                    TempData["Success"] = "User role updated successfully!";
                }
                else
                {
                    TempData["Error"] = "Could not update role: " +
                        string.Join(", ", result.Errors.Select(e => e.Description));
                }

                return RedirectToAction("Users");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user role");
                TempData["Error"] = "Error updating user role.";
                return RedirectToAction("Users");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    TempData["Error"] = "User not found.";
                    return RedirectToAction("Users");
                }

                // Prevent admin from deleting themselves (compare ids, not names)
                if (user.Id == _userManager.GetUserId(User))
                {
                    TempData["Error"] = "You cannot delete your own account.";
                    return RedirectToAction("Users");
                }

                var result = await _userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    TempData["Success"] = "User deleted successfully!";
                }
                else
                {
                    TempData["Error"] = "Error deleting user: " +
                        string.Join(", ", result.Errors.Select(e => e.Description));
                }

                return RedirectToAction("Users");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user");
                TempData["Error"] = "Error deleting user. They may have orders or reports linked to them.";
                return RedirectToAction("Users");
            }
        }

        // ---------------------------------------------------------------
        // PRODUCT MANAGEMENT
        // ---------------------------------------------------------------
        public async Task<IActionResult> Products()
        {
            var products = await _context.Products.OrderByDescending(p => p.Id).ToListAsync();
            return View(products);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                var product = await _context.Products.FindAsync(id);
                if (product == null)
                {
                    TempData["Error"] = "Product not found.";
                    return RedirectToAction("Products");
                }

                _context.Products.Remove(product);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Product deleted successfully!";
                return RedirectToAction("Products");
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error deleting product (probably linked to orders)");
                TempData["Error"] = "This product can't be deleted because it is linked to existing orders.";
                return RedirectToAction("Products");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product");
                TempData["Error"] = "Error deleting product.";
                return RedirectToAction("Products");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleProductFeatured(int id)
        {
            try
            {
                var product = await _context.Products.FindAsync(id);
                if (product == null)
                {
                    TempData["Error"] = "Product not found.";
                    return RedirectToAction("Products");
                }

                product.IsFeatured = !product.IsFeatured;
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Product {(product.IsFeatured ? "added to" : "removed from")} featured list!";
                return RedirectToAction("Products");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling product featured status");
                TempData["Error"] = "Error updating product.";
                return RedirectToAction("Products");
            }
        }

        // ---------------------------------------------------------------
        // RVM MANAGEMENT
        // ---------------------------------------------------------------
        public async Task<IActionResult> RVMs()
        {
            var rvms = await _context.RVMs.OrderByDescending(r => r.Id).ToListAsync();
            return View(rvms);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleRVMStatus(int id)
        {
            try
            {
                var rvm = await _context.RVMs.FindAsync(id);
                if (rvm == null)
                {
                    TempData["Error"] = "RVM not found.";
                    return RedirectToAction("RVMs");
                }

                rvm.IsActive = !rvm.IsActive;
                await _context.SaveChangesAsync();

                TempData["Success"] = $"RVM {rvm.MachineName} is now {(rvm.IsActive ? "active" : "inactive")}";
                return RedirectToAction("RVMs");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling RVM status");
                TempData["Error"] = "Error updating RVM status.";
                return RedirectToAction("RVMs");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRVM(int id)
        {
            try
            {
                var rvm = await _context.RVMs.FindAsync(id);
                if (rvm == null)
                {
                    TempData["Error"] = "RVM not found.";
                    return RedirectToAction("RVMs");
                }

                _context.RVMs.Remove(rvm);
                await _context.SaveChangesAsync();

                TempData["Success"] = "RVM deleted successfully!";
                return RedirectToAction("RVMs");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting RVM");
                TempData["Error"] = "Error deleting RVM.";
                return RedirectToAction("RVMs");
            }
        }

        // ---------------------------------------------------------------
        // SYSTEM MANAGEMENT
        // Renamed from System() because the name clashes with the System namespace.
        // Rename Views/Admin/System.cshtml to SystemInfo.cshtml and update any
        // asp-action="System" links to asp-action="SystemInfo".
        // ---------------------------------------------------------------
        public async Task<IActionResult> SystemInfo()
        {
            var connection = _context.Database.GetDbConnection();
            var serverVersion = "Unknown";

            try
            {
                var wasClosed = connection.State != ConnectionState.Open;
                if (wasClosed) await connection.OpenAsync();
                serverVersion = connection.ServerVersion;
                if (wasClosed) await connection.CloseAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not read database server version");
            }

            var model = new SystemInfoViewModel
            {
                DatabaseName = connection.Database ?? "Unknown",
                ServerVersion = serverVersion ?? "Unknown",
                Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SeedDatabase()
        {
            try
            {
                await RVMWMApp.Data.SeedData.Initialize(HttpContext.RequestServices);
                TempData["Success"] = "Database seeded successfully with sample data!";
                return RedirectToAction("Dashboard");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error seeding data");
                TempData["Error"] = "Error seeding database data.";
                return RedirectToAction("SystemInfo");
            }
        }

        // ---------------------------------------------------------------
        // PRIVATE METHODS
        // ---------------------------------------------------------------
        // Real activity: latest orders and dumping reports, newest first.
        private async Task<List<string>> GetRecentActivityAsync()
        {
            var orders = await _context.Orders
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .Select(o => new { o.Id, o.OrderDate, o.Status })
                .ToListAsync();

            var reports = await _context.IllegalDumpingReports
                .OrderByDescending(r => r.ReportedAt)
                .Take(5)
                .Select(r => new { r.Id, r.ReportedAt, r.Status })
                .ToListAsync();

            var activities = orders
                .Select(o => (When: o.OrderDate, Text: $"Order #{o.Id} ({o.Status})"))
                .Concat(reports.Select(r => (When: r.ReportedAt, Text: $"Dumping report #{r.Id} ({r.Status})")))
                .OrderByDescending(a => a.When)
                .Take(5)
                .Select(a => $"{a.When:dd MMM HH:mm} - {a.Text}")
                .ToList();

            if (!activities.Any())
            {
                activities.Add("No recent activity yet.");
            }

            return activities;
        }
    }
}