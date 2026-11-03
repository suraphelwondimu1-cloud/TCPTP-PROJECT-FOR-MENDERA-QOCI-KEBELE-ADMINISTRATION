using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MenderaQociDFMS.Services;

namespace MenderaQociDFMS.Controllers
{
    [Authorize]
    public class ReportsController : Controller
    {
        private readonly ReportService _reportService;

        public ReportsController(ReportService reportService)
        {
            _reportService = reportService;
        }

        public async Task<IActionResult> Index()
        {
            var residentReport = await _reportService.GetResidentReportAsync();
            var idReport = await _reportService.GetIdReportAsync();
            var expiringReport = await _reportService.GetExpiringIdReportAsync(30);
            var correspondenceReport = await _reportService.GetCorrespondenceReportAsync();

            ViewBag.ResidentReport = residentReport;
            ViewBag.IdReport = idReport;
            ViewBag.ExpiringReport = expiringReport;
            ViewBag.CorrespondenceReport = correspondenceReport;

            return View();
        }

        public async Task<IActionResult> ResidentSummary()
        {
            var report = await _reportService.GetResidentReportAsync();
            return View(report);
        }

        public async Task<IActionResult> KebeleIdSummary()
        {
            var report = await _reportService.GetIdReportAsync();
            return View(report);
        }

        public async Task<IActionResult> ExpiringIds()
        {
            var report = await _reportService.GetExpiringIdReportAsync(30);
            return View(report);
        }

        public async Task<IActionResult> CorrespondenceSummary()
        {
            var report = await _reportService.GetCorrespondenceReportAsync();
            return View(report);
        }
    }
}
