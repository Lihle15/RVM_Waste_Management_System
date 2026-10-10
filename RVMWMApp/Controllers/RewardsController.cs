using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RVMWMApp.Data;
using RVMWMApp.Models;
using RVMWMApp.ViewModels;

namespace RVMWMApp.Controllers
{
    [Authorize]
    public class RewardsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public RewardsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var rewards = await _context.Rewards
                .Where(r => r.UserId == user.Id)
                .OrderByDescending(r => r.EarnedAt)
                .ToListAsync();

            var badges = await _context.Badges.ToListAsync();

            var model = new RewardsViewModel
            {
                User = user,
                Rewards = rewards,
                Badges = badges,
                TotalPoints = rewards.Where(r => !r.IsRedeemed).Sum(r => r.Points)
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RedeemPoints(int points)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Challenge();

            // Without this check, posting a negative number ADDS points
            if (points <= 0)
            {
                TempData["Error"] = "Please enter a valid number of points to redeem.";
                return RedirectToAction("Index");
            }

            var availablePoints = await _context.Rewards
                .Where(r => r.UserId == userId && !r.IsRedeemed)
                .SumAsync(r => r.Points);

            if (points > availablePoints)
            {
                TempData["Error"] = "Insufficient points for redemption.";
                return RedirectToAction("Index");
            }

            // Negative entry that offsets the points being spent
            var redemption = new Reward
            {
                UserId = userId,
                Type = "Redemption",
                Description = $"Redeemed {points} points",
                Points = -points,
                EarnedAt = DateTime.Now
            };

            _context.Rewards.Add(redemption);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Successfully redeemed {points} points!";
            return RedirectToAction("Index");
        }
    }
}