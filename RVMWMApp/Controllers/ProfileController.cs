using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RVMWMApp.Data;
using RVMWMApp.Models;
using RVMWMApp.ViewModels;

namespace RVMWMApp.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public ProfileController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            // Calculate environmental impact (example factors)
            var impact = new EnvironmentalImpact
            {
                RecycledWeight = user.TotalRecycledWeight,
                CarbonReduced = user.TotalRecycledWeight * 2.5m,
                TreesSaved = user.TotalRecycledWeight * 0.02m,
                EnergySaved = user.TotalRecycledWeight * 15.7m
            };

            var model = new ProfileViewModel
            {
                User = user,
                Impact = impact
            };

            return View(model);
        }

        public async Task<IActionResult> Edit()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            return View(user);
        }

        // [Bind] means only these four fields can ever be taken from the form,
        // so nobody can post their own Email, UserName, points, etc.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([Bind("FullName,Address,DateOfBirth,PhoneNumber")] ApplicationUser model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            user.FullName = model.FullName;
            user.Address = model.Address;
            user.DateOfBirth = model.DateOfBirth;
            user.PhoneNumber = model.PhoneNumber;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                TempData["Success"] = "Profile updated successfully!";
                return RedirectToAction("Index");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }
    }
}