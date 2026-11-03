using System.Text;
using Microsoft.EntityFrameworkCore;
using MenderaQociDFMS.Data;

namespace MenderaQociDFMS.Services
{
    public class BackupService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<BackupService> _logger;

        public BackupService(ApplicationDbContext context, IWebHostEnvironment env, ILogger<BackupService> logger)
        {
            _context = context;
            _env = env;
            _logger = logger;
        }

        public async Task<(bool Success, string Message, string FilePath)> CreateBackupAsync()
        {
            try
            {
                var backupsFolder = Path.Combine(_env.ContentRootPath, "Backups");
                if (!Directory.Exists(backupsFolder))
                {
                    Directory.CreateDirectory(backupsFolder);
                }

                var fileName = $"MenderaQociDFMS_Backup_{DateTime.Now:yyyyMMdd_HHmmss}.sql";
                var filePath = Path.Combine(backupsFolder, fileName);

                var sb = new StringBuilder();
                sb.AppendLine($"-- Mendera Qoci Kebele DFMS Database Backup");
                sb.AppendLine($"-- Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                sb.AppendLine($"-- Host: Local Server");
                sb.AppendLine();

                // Export SubKebeles
                var subKebeles = await _context.SubKebeles.AsNoTracking().ToListAsync();
                sb.AppendLine($"-- Table: SubKebeles ({subKebeles.Count} records)");
                foreach (var sk in subKebeles)
                {
                    sb.AppendLine($"INSERT INTO SubKebeles (SubKebeleId, SubKebeleName, Description, IsActive) VALUES ({sk.SubKebeleId}, '{Escape(sk.SubKebeleName)}', '{Escape(sk.Description)}', {(sk.IsActive ? 1 : 0)});");
                }
                sb.AppendLine();

                // Export Residents
                var residents = await _context.Residents.AsNoTracking().ToListAsync();
                sb.AppendLine($"-- Table: Residents ({residents.Count} records)");
                foreach (var r in residents)
                {
                    sb.AppendLine($"INSERT INTO Residents (ResidentId, FirstName, LastName, DateOfBirth, Gender, PhoneNumber, SubKebeleId, HouseholdHead, RegistrationDate, Status) VALUES ({r.ResidentId}, '{Escape(r.FirstName)}', '{Escape(r.LastName)}', '{r.DateOfBirth:yyyy-MM-dd}', '{Escape(r.Gender)}', '{Escape(r.PhoneNumber)}', {r.SubKebeleId}, '{Escape(r.HouseholdHead)}', '{r.RegistrationDate:yyyy-MM-dd HH:mm:ss}', '{Escape(r.Status)}');");
                }
                sb.AppendLine();

                // Export Kebele ID Records
                var idRecords = await _context.KebeleIdRecords.AsNoTracking().ToListAsync();
                sb.AppendLine($"-- Table: KebeleIdRecords ({idRecords.Count} records)");
                foreach (var id in idRecords)
                {
                    sb.AppendLine($"INSERT INTO KebeleIdRecords (IdRecordId, ResidentId, IdNumber, IssueDate, ExpiryDate, IdStatus, IssuedByUserId, Remarks) VALUES ({id.IdRecordId}, {id.ResidentId}, '{Escape(id.IdNumber)}', '{id.IssueDate:yyyy-MM-dd}', '{id.ExpiryDate:yyyy-MM-dd}', '{Escape(id.IdStatus)}', '{Escape(id.IssuedByUserId)}', '{Escape(id.Remarks)}');");
                }
                sb.AppendLine();

                // Export Correspondences
                var correspondences = await _context.Correspondences.AsNoTracking().ToListAsync();
                sb.AppendLine($"-- Table: Correspondences ({correspondences.Count} records)");
                foreach (var c in correspondences)
                {
                    sb.AppendLine($"INSERT INTO Correspondences (CorrespondenceId, ResidentId, DocumentType, IssueDate, Purpose, IssuedByUserId, ReferenceNumber) VALUES ({c.CorrespondenceId}, {c.ResidentId}, '{Escape(c.DocumentType)}', '{c.IssueDate:yyyy-MM-dd}', '{Escape(c.Purpose)}', '{Escape(c.IssuedByUserId)}', '{Escape(c.ReferenceNumber)}');");
                }
                sb.AppendLine();

                await File.WriteAllTextAsync(filePath, sb.ToString(), Encoding.UTF8);

                _logger.LogInformation("Database backup successfully created at {FilePath}", filePath);
                return (true, $"Backup successfully generated ({fileName}).", filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create database backup.");
                return (false, $"Backup failed: {ex.Message}", string.Empty);
            }
        }

        private static string Escape(string? input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;
            return input.Replace("'", "''");
        }
    }
}
