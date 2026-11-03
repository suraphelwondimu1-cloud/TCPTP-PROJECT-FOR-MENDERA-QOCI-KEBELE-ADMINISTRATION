using Microsoft.EntityFrameworkCore;
using MenderaQociDFMS.Data;
using MenderaQociDFMS.ViewModels;

namespace MenderaQociDFMS.Services
{
    public class ReportService
    {
        private readonly ApplicationDbContext _context;

        public ReportService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ResidentReportViewModel> GetResidentReportAsync()
        {
            var total = await _context.Residents.CountAsync();
            var active = await _context.Residents.CountAsync(r => r.Status == "Active");
            var inactive = await _context.Residents.CountAsync(r => r.Status != "Active");

            var subKebeleCounts = await _context.SubKebeles
                .Select(sk => new SubKebeleResidentCount
                {
                    SubKebeleName = sk.SubKebeleName,
                    Count = sk.Residents.Count
                })
                .ToListAsync();

            return new ResidentReportViewModel
            {
                TotalResidents = total,
                ActiveResidents = active,
                InactiveResidents = inactive,
                BySubKebele = subKebeleCounts
            };
        }

        public async Task<IdReportViewModel> GetIdReportAsync()
        {
            var total = await _context.KebeleIdRecords.CountAsync();
            var active = await _context.KebeleIdRecords.CountAsync(k => k.IdStatus == "Active" && k.ExpiryDate >= DateTime.Now);
            var expired = await _context.KebeleIdRecords.CountAsync(k => k.IdStatus == "Expired" || k.ExpiryDate < DateTime.Now);
            var cancelled = await _context.KebeleIdRecords.CountAsync(k => k.IdStatus == "Cancelled");

            return new IdReportViewModel
            {
                TotalIds = total,
                ActiveIds = active,
                ExpiredIds = expired,
                CancelledIds = cancelled
            };
        }

        public async Task<ExpiringIdReportViewModel> GetExpiringIdReportAsync(int daysThreshold = 30)
        {
            var targetDate = DateTime.Now.AddDays(daysThreshold);

            var expiringRecords = await _context.KebeleIdRecords
                .Include(k => k.Resident)
                .ThenInclude(r => r!.SubKebele)
                .Where(k => k.IdStatus == "Active" && k.ExpiryDate >= DateTime.Now && k.ExpiryDate <= targetDate)
                .OrderBy(k => k.ExpiryDate)
                .ToListAsync();

            return new ExpiringIdReportViewModel
            {
                TotalExpiringSoon = expiringRecords.Count,
                ExpiringRecords = expiringRecords
            };
        }

        public async Task<CorrespondenceReportViewModel> GetCorrespondenceReportAsync()
        {
            var total = await _context.Correspondences.CountAsync();

            var docCounts = await _context.Correspondences
                .GroupBy(c => c.DocumentType)
                .Select(g => new DocumentTypeCount
                {
                    DocumentType = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            return new CorrespondenceReportViewModel
            {
                TotalCorrespondences = total,
                ByDocumentType = docCounts
            };
        }
    }
}
