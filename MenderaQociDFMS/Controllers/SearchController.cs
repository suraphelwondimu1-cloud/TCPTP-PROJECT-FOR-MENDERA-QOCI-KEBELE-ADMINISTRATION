using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MenderaQociDFMS.Data;
using MenderaQociDFMS.Models;

namespace MenderaQociDFMS.Controllers
{
    [Authorize]
    public class SearchController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SearchController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? q)
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                return View(new List<Resident>());
            }

            q = q.Trim();

            var results = await _context.Residents
                .Include(r => r.SubKebele)
                .Include(r => r.KebeleIdRecords)
                .Include(r => r.Correspondences)
                .Where(r => r.FirstName.Contains(q) ||
                            r.LastName.Contains(q) ||
                            (r.PhoneNumber != null && r.PhoneNumber.Contains(q)) ||
                            r.SubKebele!.SubKebeleName.Contains(q) ||
                            r.ResidentId.ToString() == q ||
                            r.KebeleIdRecords.Any(k => k.IdNumber.Contains(q)) ||
                            r.Correspondences.Any(c => c.ReferenceNumber.Contains(q)))
                .OrderBy(r => r.FirstName)
                .ToListAsync();

            ViewBag.Query = q;
            return View(results);
        }
    }
}
