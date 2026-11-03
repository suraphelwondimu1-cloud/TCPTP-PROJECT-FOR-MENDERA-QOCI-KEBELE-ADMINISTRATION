using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MenderaQociDFMS.Data;
using MenderaQociDFMS.Models;

namespace MenderaQociDFMS.Controllers
{
    [Authorize]
    public class ResidentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ResidentsController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: Residents
        public async Task<IActionResult> Index(string? searchString, int? subKebeleId, string? status, int page = 1)
        {
            int pageSize = 10;
            var query = _context.Residents
                .Include(r => r.SubKebele)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                searchString = searchString.Trim();
                query = query.Where(r => r.FirstName.Contains(searchString) ||
                                         r.LastName.Contains(searchString) ||
                                         (r.PhoneNumber != null && r.PhoneNumber.Contains(searchString)) ||
                                         r.ResidentId.ToString() == searchString);
            }

            if (subKebeleId.HasValue && subKebeleId.Value > 0)
            {
                query = query.Where(r => r.SubKebeleId == subKebeleId.Value);
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(r => r.Status == status);
            }

            var totalCount = await query.CountAsync();
            var items = await query.OrderByDescending(r => r.RegistrationDate)
                                   .Skip((page - 1) * pageSize)
                                   .Take(pageSize)
                                   .ToListAsync();

            ViewBag.SearchString = searchString;
            ViewBag.SubKebeleId = subKebeleId;
            ViewBag.Status = status;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            ViewBag.SubKebeles = new SelectList(await _context.SubKebeles.Where(s => s.IsActive).ToListAsync(), "SubKebeleId", "SubKebeleName", subKebeleId);

            return View(items);
        }

        // GET: Residents/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var resident = await _context.Residents
                .Include(r => r.SubKebele)
                .Include(r => r.KebeleIdRecords)
                .Include(r => r.Correspondences)
                .FirstOrDefaultAsync(m => m.ResidentId == id);

            if (resident == null) return NotFound();

            return View(resident);
        }

        // GET: Residents/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.SubKebeleId = new SelectList(await _context.SubKebeles.Where(s => s.IsActive).ToListAsync(), "SubKebeleId", "SubKebeleName");
            return View(new Resident { DateOfBirth = DateTime.Now.AddYears(-25) });
        }

        // POST: Residents/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FirstName,LastName,DateOfBirth,Gender,PhoneNumber,SubKebeleId,HouseholdHead,Status")] Resident resident, IFormFile? photoFile, string? photoDataUrl)
        {
            if (resident.DateOfBirth > DateTime.Now)
            {
                ModelState.AddModelError("DateOfBirth", "Date of birth cannot be in the future.");
            }

            if (ModelState.IsValid)
            {
                resident.RegistrationDate = DateTime.Now;

                // Process photo from file upload or camera capture
                var savedPhotoPath = await SavePhotoAsync(photoFile, photoDataUrl);
                if (!string.IsNullOrEmpty(savedPhotoPath))
                {
                    resident.PhotoPath = savedPhotoPath;
                }

                _context.Add(resident);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Resident '{resident.FullName}' successfully registered.";
                return RedirectToAction(nameof(Details), new { id = resident.ResidentId });
            }

            ViewBag.SubKebeleId = new SelectList(await _context.SubKebeles.Where(s => s.IsActive).ToListAsync(), "SubKebeleId", "SubKebeleName", resident.SubKebeleId);
            return View(resident);
        }

        // GET: Residents/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var resident = await _context.Residents.FindAsync(id);
            if (resident == null) return NotFound();

            ViewBag.SubKebeleId = new SelectList(await _context.SubKebeles.Where(s => s.IsActive).ToListAsync(), "SubKebeleId", "SubKebeleName", resident.SubKebeleId);
            return View(resident);
        }

        // POST: Residents/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ResidentId,FirstName,LastName,DateOfBirth,Gender,PhoneNumber,SubKebeleId,HouseholdHead,RegistrationDate,Status,PhotoPath")] Resident resident, IFormFile? photoFile, string? photoDataUrl, bool removePhoto = false)
        {
            if (id != resident.ResidentId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var existingResident = await _context.Residents.AsNoTracking().FirstOrDefaultAsync(r => r.ResidentId == id);
                    if (existingResident == null) return NotFound();

                    if (removePhoto)
                    {
                        DeletePhotoFile(existingResident.PhotoPath);
                        resident.PhotoPath = null;
                    }
                    else
                    {
                        var newPhotoPath = await SavePhotoAsync(photoFile, photoDataUrl);
                        if (!string.IsNullOrEmpty(newPhotoPath))
                        {
                            DeletePhotoFile(existingResident.PhotoPath);
                            resident.PhotoPath = newPhotoPath;
                        }
                        else
                        {
                            resident.PhotoPath = existingResident.PhotoPath;
                        }
                    }

                    _context.Update(resident);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = $"Resident '{resident.FullName}' information updated.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ResidentExists(resident.ResidentId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Details), new { id = resident.ResidentId });
            }

            ViewBag.SubKebeleId = new SelectList(await _context.SubKebeles.Where(s => s.IsActive).ToListAsync(), "SubKebeleId", "SubKebeleName", resident.SubKebeleId);
            return View(resident);
        }

        // Helper to process uploaded file or camera captured data URL
        private async Task<string?> SavePhotoAsync(IFormFile? file, string? dataUrl)
        {
            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "photos");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // Option 1: File uploaded from computer
            if (file != null && file.Length > 0)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(extension))
                {
                    return null;
                }

                // Max 10MB limit
                if (file.Length > 10 * 1024 * 1024)
                {
                    return null;
                }

                var fileName = $"resident_{Guid.NewGuid():N}{extension}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                return $"/uploads/photos/{fileName}";
            }

            // Option 2: Captured from camera (base64 data URL)
            if (!string.IsNullOrWhiteSpace(dataUrl) && dataUrl.StartsWith("data:image/"))
            {
                try
                {
                    var commaIndex = dataUrl.IndexOf(',');
                    if (commaIndex > 0)
                    {
                        var header = dataUrl.Substring(0, commaIndex);
                        var base64Data = dataUrl.Substring(commaIndex + 1);
                        var bytes = Convert.FromBase64String(base64Data);

                        if (bytes.Length > 10 * 1024 * 1024)
                        {
                            return null;
                        }

                        var extension = ".png";
                        if (header.Contains("jpeg") || header.Contains("jpg")) extension = ".jpg";
                        else if (header.Contains("webp")) extension = ".webp";

                        var fileName = $"resident_cam_{Guid.NewGuid():N}{extension}";
                        var filePath = Path.Combine(uploadsFolder, fileName);
                        await System.IO.File.WriteAllBytesAsync(filePath, bytes);

                        return $"/uploads/photos/{fileName}";
                    }
                }
                catch
                {
                    return null;
                }
            }

            return null;
        }

        private void DeletePhotoFile(string? photoPath)
        {
            if (string.IsNullOrWhiteSpace(photoPath)) return;
            try
            {
                var trimmedPath = photoPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
                var fullPath = Path.Combine(_env.WebRootPath, trimmedPath);
                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                }
            }
            catch
            {
                // Non-critical file cleanup error
            }
        }

        private bool ResidentExists(int id)
        {
            return _context.Residents.Any(e => e.ResidentId == id);
        }
    }
}
