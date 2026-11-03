using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MenderaQociDFMS.Data;
using MenderaQociDFMS.Models;
using MenderaQociDFMS.ViewModels;

namespace MenderaQociDFMS.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var now = DateTime.Now;
            var expiringThreshold = now.AddDays(30);

            var totalResidents = await _context.Residents.CountAsync();
            var activeResidents = await _context.Residents.CountAsync(r => r.Status == "Active");
            
            var activeIds = await _context.KebeleIdRecords.CountAsync(k => k.IdStatus == "Active" && k.ExpiryDate >= now);
            var expiredIds = await _context.KebeleIdRecords.CountAsync(k => k.IdStatus == "Expired" || k.ExpiryDate < now);
            var expiringSoon = await _context.KebeleIdRecords.CountAsync(k => k.IdStatus == "Active" && k.ExpiryDate >= now && k.ExpiryDate <= expiringThreshold);

            var totalCorrespondences = await _context.Correspondences.CountAsync();
            var staffUsersCount = (await _userManager.GetUsersInRoleAsync("Staff")).Count;

            var recentResidents = await _context.Residents
                .Include(r => r.SubKebele)
                .OrderByDescending(r => r.RegistrationDate)
                .Take(5)
                .ToListAsync();

            var recentIds = await _context.KebeleIdRecords
                .Include(k => k.Resident)
                .OrderByDescending(k => k.IssueDate)
                .Take(5)
                .ToListAsync();

            var viewModel = new DashboardViewModel
            {
                TotalResidents = totalResidents,
                ActiveResidents = activeResidents,
                ActiveKebeleIds = activeIds,
                ExpiredKebeleIds = expiredIds,
                IdsExpiringSoon = expiringSoon,
                TotalCorrespondences = totalCorrespondences,
                StaffUsersCount = staffUsersCount,
                RecentResidents = recentResidents,
                RecentIdRecords = recentIds
            };

            return View(viewModel);
        }
    }
}
