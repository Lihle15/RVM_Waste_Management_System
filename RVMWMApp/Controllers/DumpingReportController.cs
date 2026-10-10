using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RVMWMApp.Data;
using RVMWMApp.Models;

namespace RVMWMApp.Controllers
{
    [Authorize]
    public class DumpingReportController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DumpingReportController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var reports = await _context.IllegalDumpingReports
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.ReportedAt)
                .ToListAsync();

            return View(reports);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(IllegalDumpingReport report)
        {
            // These are set by the server, not the form. Without removing them,
            // non-nullable properties make ModelState invalid and the form
            // silently redisplays without saving.
            ModelState.Remove("User");
            ModelState.Remove("UserId");
            ModelState.Remove("Status");
            ModelState.Remove("ReportedAt");
            ModelState.Remove("AdminNotes");

            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User);
                if (userId == null)
                {
                    ModelState.AddModelError(string.Empty, "Unable to determine the current user.");
                    return View(report);
                }

                // Overwrite anything the browser might have posted for these fields
                report.Id = 0;
                report.UserId = userId;
                report.ReportedAt = DateTime.Now;
                report.Status = "Reported";
                report.AdminNotes = null;

                _context.IllegalDumpingReports.Add(report);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Illegal dumping report submitted successfully! Our team will review it.";
                return RedirectToAction("Index");
            }
            return View(report);
        }

        [Authorize(Roles = "Admin,Operator")]
        public async Task<IActionResult> Manage()
        {
            var reports = await _context.IllegalDumpingReports
                .Include(r => r.User)
                .OrderByDescending(r => r.ReportedAt)
                .ToListAsync();
            return View(reports);
        }

        [Authorize(Roles = "Admin,Operator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status, string? adminNotes)
        {
            if (string.IsNullOrWhiteSpace(status) || status.Length > 50)
            {
                TempData["Error"] = "Please choose a valid status.";
                return RedirectToAction("Manage");
            }

            var report = await _context.IllegalDumpingReports.FindAsync(id);
            if (report == null)
            {
                TempData["Error"] = "Report not found.";
                return RedirectToAction("Manage");
            }

            report.Status = status;
            report.AdminNotes = adminNotes;
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Report status updated to {status}";

            return RedirectToAction("Manage");
        }
    }
}