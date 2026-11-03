using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MenderaQociDFMS.Data;
using MenderaQociDFMS.Models;

namespace MenderaQociDFMS.Controllers
{
    [Authorize(Roles = "Admin")]
    public class SubKebelesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SubKebelesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: SubKebeles
        public async Task<IActionResult> Index()
        {
            var subKebeles = await _context.SubKebeles
                .Include(s => s.Residents)
                .ToListAsync();

            return View(subKebeles);
        }

        // GET: SubKebeles/Create
        public IActionResult Create()
        {
            return View(new SubKebele());
        }

        // POST: SubKebeles/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("SubKebeleName,Description,IsActive")] SubKebele subKebele)
        {
            if (await _context.SubKebeles.AnyAsync(s => s.SubKebeleName == subKebele.SubKebeleName))
            {
                ModelState.AddModelError("SubKebeleName", "A Sub-Kebele with this name already exists.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(subKebele);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Sub-Kebele '{subKebele.SubKebeleName}' added.";
                return RedirectToAction(nameof(Index));
            }

            return View(subKebele);
        }

        // GET: SubKebeles/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var subKebele = await _context.SubKebeles.FindAsync(id);
            if (subKebele == null) return NotFound();

            return View(subKebele);
        }

        // POST: SubKebeles/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("SubKebeleId,SubKebeleName,Description,IsActive")] SubKebele subKebele)
        {
            if (id != subKebele.SubKebeleId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(subKebele);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = $"Sub-Kebele '{subKebele.SubKebeleName}' updated.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SubKebeleExists(subKebele.SubKebeleId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            return View(subKebele);
        }

        // POST: SubKebeles/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var subKebele = await _context.SubKebeles
                .Include(s => s.Residents)
                .FirstOrDefaultAsync(s => s.SubKebeleId == id);

            if (subKebele == null) return NotFound();

            // Constraint check: prevent deletion if residents are linked
            if (subKebele.Residents.Any())
            {
                TempData["Error"] = $"Cannot delete Sub-Kebele '{subKebele.SubKebeleName}' because {subKebele.Residents.Count} residents are currently assigned to it. Please reassign residents first.";
                return RedirectToAction(nameof(Index));
            }

            _context.SubKebeles.Remove(subKebele);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Sub-Kebele '{subKebele.SubKebeleName}' deleted.";
            return RedirectToAction(nameof(Index));
        }

        private bool SubKebeleExists(int id)
        {
            return _context.SubKebeles.Any(e => e.SubKebeleId == id);
        }
    }
}
