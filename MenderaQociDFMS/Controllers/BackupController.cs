using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MenderaQociDFMS.Services;

namespace MenderaQociDFMS.Controllers
{
    [Authorize(Roles = "Admin")]
    public class BackupController : Controller
    {
        private readonly BackupService _backupService;
        private readonly IWebHostEnvironment _env;

        public BackupController(BackupService backupService, IWebHostEnvironment env)
        {
            _backupService = backupService;
            _env = env;
        }

        public IActionResult Index()
        {
            var backupsFolder = Path.Combine(_env.ContentRootPath, "Backups");
            var backupFiles = new List<FileInfo>();

            if (Directory.Exists(backupsFolder))
            {
                var dir = new DirectoryInfo(backupsFolder);
                backupFiles = dir.GetFiles("*.sql").OrderByDescending(f => f.CreationTime).ToList();
            }

            return View(backupFiles);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateBackup()
        {
            var (success, message, filePath) = await _backupService.CreateBackupAsync();

            if (success)
            {
                TempData["Success"] = message;
            }
            else
            {
                TempData["Error"] = message;
            }

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Download(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return BadRequest();

            // Sanitize filename to prevent path traversal
            fileName = Path.GetFileName(fileName);
            var filePath = Path.Combine(_env.ContentRootPath, "Backups", fileName);

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("Backup file not found.");
            }

            var bytes = System.IO.File.ReadAllBytes(filePath);
            return File(bytes, "application/sql", fileName);
        }
    }
}
