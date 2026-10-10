using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RVMWMApp.Data;
using RVMWMApp.Models;

namespace RVMWMApp.Controllers
{
    public class RVMsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RVMsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ---------------------------------------------------------------
        // PUBLIC: view machines
        // ---------------------------------------------------------------

        // GET: RVMs
        public async Task<IActionResult> Index()
        {
            return View(await _context.RVMs.ToListAsync());
        }

        public async Task<IActionResult> Locator()
        {
            var rvms = await _context.RVMs.Where(r => r.IsActive).ToListAsync();
            return View(rvms);
        }

        // GET: RVMs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reverseVendingMachine = await _context.RVMs.FirstOrDefaultAsync(m => m.Id == id);
            if (reverseVendingMachine == null)
            {
                return NotFound();
            }

            return View(reverseVendingMachine);
        }

        // ---------------------------------------------------------------
        // ADMIN ONLY: create / edit / delete
        // ---------------------------------------------------------------

        // GET: RVMs/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: RVMs/Create
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MachineName,Location,Address,City,State,ZipCode,Latitude,Longitude,Status,IsActive,Capacity,CurrentLoad,ImageUrl,LastMaintenance,NextMaintenance,AcceptsPlastic,AcceptsGlass,AcceptsMetal,AcceptsPaper,AcceptsElectronics")] ReverseVendingMachine reverseVendingMachine)
        {
            if (ModelState.IsValid)
            {
                _context.Add(reverseVendingMachine);
                await _context.SaveChangesAsync();
                TempData["Success"] = "RVM created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(reverseVendingMachine);
        }

        // GET: RVMs/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reverseVendingMachine = await _context.RVMs.FindAsync(id);
            if (reverseVendingMachine == null)
            {
                return NotFound();
            }
            return View(reverseVendingMachine);
        }

        // POST: RVMs/Edit/5
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id)
        {
            var reverseVendingMachine = await _context.RVMs.FindAsync(id);
            if (reverseVendingMachine == null)
            {
                return NotFound();
            }

            // Only fields that are actually on the form get updated, so fields the
            // form doesn't include (coordinates, image, etc.) are not wiped.
            if (await TryUpdateModelAsync(reverseVendingMachine, "",
                    r => r.MachineName, r => r.Location, r => r.Address, r => r.City,
                    r => r.State, r => r.ZipCode, r => r.Latitude, r => r.Longitude,
                    r => r.Status, r => r.IsActive, r => r.Capacity, r => r.CurrentLoad,
                    r => r.ImageUrl, r => r.LastMaintenance, r => r.NextMaintenance,
                    r => r.AcceptsPlastic, r => r.AcceptsGlass, r => r.AcceptsMetal,
                    r => r.AcceptsPaper, r => r.AcceptsElectronics))
            {
                try
                {
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "RVM updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ReverseVendingMachineExists(id))
                    {
                        return NotFound();
                    }
                    throw;
                }
            }
            return View(reverseVendingMachine);
        }

        // GET: RVMs/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reverseVendingMachine = await _context.RVMs.FirstOrDefaultAsync(m => m.Id == id);
            if (reverseVendingMachine == null)
            {
                return NotFound();
            }

            return View(reverseVendingMachine);
        }

        // POST: RVMs/Delete/5
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var reverseVendingMachine = await _context.RVMs.FindAsync(id);
            if (reverseVendingMachine == null)
            {
                TempData["Error"] = "RVM not found.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                _context.RVMs.Remove(reverseVendingMachine);
                await _context.SaveChangesAsync();
                TempData["Success"] = "RVM deleted successfully!";
            }
            catch (DbUpdateException)
            {
                TempData["Error"] = "This RVM can't be deleted because other records depend on it.";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ReverseVendingMachineExists(int id)
        {
            return _context.RVMs.Any(e => e.Id == id);
        }
    }
}