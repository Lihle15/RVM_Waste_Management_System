using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RVMWMApp.Data;
using RVMWMApp.Models;

namespace RVMWMApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var featuredProducts = await _context.Products
                .Where(p => p.IsFeatured)
                .OrderByDescending(p => p.Id)
                .Take(4)
                .ToListAsync();

            var rvms = await _context.RVMs
                .Where(r => r.IsActive)
                .OrderBy(r => r.MachineName)
                .Take(4)
                .ToListAsync();

            var viewModel = new HomeViewModel
            {
                FeaturedProducts = featuredProducts,
                RVMs = rvms
            };

            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}