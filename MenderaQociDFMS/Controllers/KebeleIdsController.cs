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
    public class KebeleIdsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public KebeleIdsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: KebeleIds
        public async Task<IActionResult> Index(string? searchNumber, string? status, int page = 1)
        {
            int pageSize = 10;
            var query = _context.KebeleIdRecords
                .Include(k => k.Resident)
                .ThenInclude(r => r!.SubKebele)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchNumber))
            {
                searchNumber = searchNumber.Trim();
                query = query.Where(k => k.IdNumber.Contains(searchNumber) || 
                                         k.Resident!.FirstName.Contains(searchNumber) ||
                                         k.Resident!.LastName.Contains(searchNumber));
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                if (status == "ExpiringSoon")
                {
                    var threshold = DateTime.Now.AddDays(30);
                    query = query.Where(k => k.IdStatus == "Active" && k.ExpiryDate >= DateTime.Now && k.ExpiryDate <= threshold);
                }
                else if (status == "Expired")
                {
                    query = query.Where(k => k.IdStatus == "Expired" || k.ExpiryDate < DateTime.Now);
                }
                else
                {
                    query = query.Where(k => k.IdStatus == status);
                }
            }

            var totalCount = await query.CountAsync();
            var items = await query.OrderByDescending(k => k.IssueDate)
                                   .Skip((page - 1) * pageSize)
                                   .Take(pageSize)
                                   .ToListAsync();

            ViewBag.SearchNumber = searchNumber;
            ViewBag.Status = status;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            return View(items);
        }

        // GET: KebeleIds/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var idRecord = await _context.KebeleIdRecords
                .Include(k => k.Resident)
                .ThenInclude(r => r!.SubKebele)
                .Include(k => k.IssuedByUser)
                .FirstOrDefaultAsync(m => m.IdRecordId == id);

            if (idRecord == null) return NotFound();

            return View(idRecord);
        }

        // GET: KebeleIds/CardPreview/5
        public async Task<IActionResult> CardPreview(int? id)
        {
            if (id == null) return NotFound();

            var idRecord = await _context.KebeleIdRecords
                .Include(k => k.Resident)
                .ThenInclude(r => r!.SubKebele)
                .Include(k => k.IssuedByUser)
                .FirstOrDefaultAsync(m => m.IdRecordId == id);

            if (idRecord == null) return NotFound();

            return View(idRecord);
        }

        // GET: KebeleIds/Create
        public async Task<IActionResult> Create(int? residentId)
        {
            var model = new KebeleIdRecord
            {
                IssueDate = DateTime.Now,
                ExpiryDate = DateTime.Now.AddYears(2),
                IdNumber = $"KID-MQ-{DateTime.Now:yyyy}-{new Random().Next(100, 999)}"
            };

            if (residentId.HasValue)
            {
                model.ResidentId = residentId.Value;
            }

            await PopulateResidentsDropDownListAsync(model.ResidentId);
            return View(model);
        }

        // POST: KebeleIds/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ResidentId,IdNumber,IssueDate,ExpiryDate,IdStatus,Remarks")] KebeleIdRecord idRecord)
        {
            if (await _context.KebeleIdRecords.AnyAsync(k => k.IdNumber == idRecord.IdNumber))
            {
                ModelState.AddModelError("IdNumber", "This Kebele ID Number already exists in the system.");
            }

            if (idRecord.ExpiryDate <= idRecord.IssueDate)
            {
                ModelState.AddModelError("ExpiryDate", "Expiry date must be after issue date.");
            }

            if (ModelState.IsValid)
            {
                var currentUser = await _userManager.GetUserAsync(User);
                idRecord.IssuedByUserId = currentUser?.Id;
                _context.Add(idRecord);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Kebele ID '{idRecord.IdNumber}' issued successfully.";
                return RedirectToAction("Details", "Residents", new { id = idRecord.ResidentId });
            }

            await PopulateResidentsDropDownListAsync(idRecord.ResidentId);
            return View(idRecord);
        }

        // GET: KebeleIds/Renew/5
        public async Task<IActionResult> Renew(int? id)
        {
            if (id == null) return NotFound();

            var oldRecord = await _context.KebeleIdRecords
                .Include(k => k.Resident)
                .FirstOrDefaultAsync(k => k.IdRecordId == id);

            if (oldRecord == null) return NotFound();

            var newRecord = new KebeleIdRecord
            {
                ResidentId = oldRecord.ResidentId,
                Resident = oldRecord.Resident,
                IdNumber = $"KID-MQ-{DateTime.Now:yyyy}-{new Random().Next(100, 999)}",
                IssueDate = DateTime.Now,
                ExpiryDate = DateTime.Now.AddYears(2),
                IdStatus = "Active",
                Remarks = $"Renewed from previous ID #{oldRecord.IdNumber}"
            };

            return View(newRecord);
        }

        // POST: KebeleIds/Renew
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Renew([Bind("ResidentId,IdNumber,IssueDate,ExpiryDate,IdStatus,Remarks")] KebeleIdRecord newRecord, int oldRecordId)
        {
            if (await _context.KebeleIdRecords.AnyAsync(k => k.IdNumber == newRecord.IdNumber))
            {
                ModelState.AddModelError("IdNumber", "This Kebele ID Number already exists.");
            }

            if (ModelState.IsValid)
            {
                // Mark previous ID as expired/renewed if active
                var previousRecord = await _context.KebeleIdRecords.FindAsync(oldRecordId);
                if (previousRecord != null && previousRecord.IdStatus == "Active")
                {
                    previousRecord.IdStatus = "Expired";
                    _context.Update(previousRecord);
                }

                var currentUser = await _userManager.GetUserAsync(User);
                newRecord.IssuedByUserId = currentUser?.Id;

                _context.Add(newRecord);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Kebele ID successfully renewed. New ID Number: {newRecord.IdNumber}";
                return RedirectToAction("Details", "Residents", new { id = newRecord.ResidentId });
            }

            newRecord.Resident = await _context.Residents.FindAsync(newRecord.ResidentId);
            return View(newRecord);
        }

        private async Task PopulateResidentsDropDownListAsync(object? selectedResident = null)
        {
            var residentsQuery = from r in _context.Residents
                                 where r.Status == "Active"
                                 orderby r.FirstName
                                 select new
                                 {
                                     r.ResidentId,
                                     FullNameWithId = $"{r.FirstName} {r.LastName} (ID: {r.ResidentId})"
                                 };

            ViewBag.ResidentId = new SelectList(await residentsQuery.ToListAsync(), "ResidentId", "FullNameWithId", selectedResident);
        }
    }
}
