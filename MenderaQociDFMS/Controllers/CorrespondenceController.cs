using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MenderaQociDFMS.Data;
using MenderaQociDFMS.Models;

namespace MenderaQociDFMS.Controllers
{
    [Authorize]
    public class CorrespondenceController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CorrespondenceController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Correspondence
        public async Task<IActionResult> Index(string? searchString, string? docType, int page = 1)
        {
            int pageSize = 10;
            var query = _context.Correspondences
                .Include(c => c.Resident)
                .ThenInclude(r => r!.SubKebele)
                .Include(c => c.IssuedByUser)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                searchString = searchString.Trim();
                query = query.Where(c => c.ReferenceNumber.Contains(searchString) ||
                                         c.Purpose.Contains(searchString) ||
                                         c.Resident!.FirstName.Contains(searchString) ||
                                         c.Resident!.LastName.Contains(searchString));
            }

            if (!string.IsNullOrWhiteSpace(docType))
            {
                query = query.Where(c => c.DocumentType == docType);
            }

            var totalCount = await query.CountAsync();
            var items = await query.OrderByDescending(c => c.IssueDate)
                                   .Skip((page - 1) * pageSize)
                                   .Take(pageSize)
                                   .ToListAsync();

            ViewBag.SearchString = searchString;
            ViewBag.DocType = docType;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            ViewBag.DocumentTypes = new List<string> { "Residence Certificate", "Support Letter", "ID Verification Letter", "Marital Status Certificate", "Other Official Letter" };

            return View(items);
        }

        // GET: Correspondence/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var item = await _context.Correspondences
                .Include(c => c.Resident)
                .ThenInclude(r => r!.SubKebele)
                .Include(c => c.IssuedByUser)
                .FirstOrDefaultAsync(m => m.CorrespondenceId == id);

            if (item == null) return NotFound();

            return View(item);
        }

        // GET: Correspondence/Create
        public async Task<IActionResult> Create(int? residentId)
        {
            var model = new Correspondence
            {
                IssueDate = DateTime.Now,
                ReferenceNumber = $"COR-MQ-{DateTime.Now:yyyy}-{new Random().Next(100, 999)}"
            };

            if (residentId.HasValue)
            {
                model.ResidentId = residentId.Value;
            }

            await PopulateResidentsDropDownListAsync(model.ResidentId);
            ViewBag.DocumentTypes = new SelectList(new List<string> { "Residence Certificate", "Support Letter", "ID Verification Letter", "Marital Status Certificate", "Other Official Letter" });

            return View(model);
        }

        // POST: Correspondence/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ResidentId,DocumentType,IssueDate,Purpose,ReferenceNumber")] Correspondence correspondence)
        {
            if (await _context.Correspondences.AnyAsync(c => c.ReferenceNumber == correspondence.ReferenceNumber))
            {
                ModelState.AddModelError("ReferenceNumber", "This Reference Number already exists.");
            }

            if (ModelState.IsValid)
            {
                var currentUser = await _userManager.GetUserAsync(User);
                correspondence.IssuedByUserId = currentUser?.Id;
                _context.Add(correspondence);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Correspondence document #{correspondence.ReferenceNumber} recorded successfully.";
                return RedirectToAction("Details", "Residents", new { id = correspondence.ResidentId });
            }

            await PopulateResidentsDropDownListAsync(correspondence.ResidentId);
            ViewBag.DocumentTypes = new SelectList(new List<string> { "Residence Certificate", "Support Letter", "ID Verification Letter", "Marital Status Certificate", "Other Official Letter" }, correspondence.DocumentType);
            return View(correspondence);
        }

        private async Task PopulateResidentsDropDownListAsync(object? selectedResident = null)
        {
            var query = from r in _context.Residents
                        where r.Status == "Active"
                        orderby r.FirstName
                        select new
                        {
                            r.ResidentId,
                            FullNameWithId = $"{r.FirstName} {r.LastName} (ID: {r.ResidentId})"
                        };

            ViewBag.ResidentId = new SelectList(await query.ToListAsync(), "ResidentId", "FullNameWithId", selectedResident);
        }
    }
}
