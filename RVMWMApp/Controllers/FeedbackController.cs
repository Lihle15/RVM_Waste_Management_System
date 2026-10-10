using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RVMWMApp.Data;
using RVMWMApp.Models;

namespace RVMWMApp.Controllers
{
    [Authorize]
    public class FeedbackController : Controller
    {
        // true  = reviews appear straight away
        // false = reviews stay hidden until an admin approves them on the Manage page
        private const bool AutoApproveFeedback = true;

        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public FeedbackController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var feedback = await _context.Feedback
                .Include(f => f.User)
                .Where(f => f.IsApproved)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();
            return View(feedback);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Feedback feedback)
        {
            // Server-controlled fields: remove them from validation so the form
            // doesn't silently fail on "required" navigation properties.
            ModelState.Remove("User");
            ModelState.Remove("UserId");
            ModelState.Remove("CreatedAt");
            ModelState.Remove("IsApproved");

            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                {
                    TempData["Error"] = "You must be logged in to submit a review.";
                    return RedirectToAction("Index");
                }

                feedback.Id = 0;
                feedback.UserId = userId;
                feedback.CreatedAt = DateTime.Now;
                feedback.IsApproved = AutoApproveFeedback;

                _context.Feedback.Add(feedback);
                await _context.SaveChangesAsync();

                TempData["Success"] = AutoApproveFeedback
                    ? "Thank you for your feedback! Your review has been submitted."
                    : "Thank you for your feedback! Your review will appear once it has been approved.";
                return RedirectToAction("Index");
            }

            // Something failed; redisplay the form
            return View(feedback);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Manage()
        {
            var feedback = await _context.Feedback
                .Include(f => f.User)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();
            return View(feedback);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleApproval(int id)
        {
            var feedback = await _context.Feedback.FindAsync(id);
            if (feedback != null)
            {
                feedback.IsApproved = !feedback.IsApproved;
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Review {(feedback.IsApproved ? "approved" : "unapproved")} successfully.";
            }
            return RedirectToAction("Manage");
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var feedback = await _context.Feedback.FindAsync(id);
            if (feedback != null)
            {
                _context.Feedback.Remove(feedback);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Review deleted successfully.";
            }
            return RedirectToAction("Manage");
        }
    }
}