using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MenderaQociDFMS.Models;

namespace MenderaQociDFMS.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Ensure database created
            await context.Database.EnsureCreatedAsync();

            // Check and add PhotoPath column to Residents if it does not exist (for existing SQLite databases)
            try
            {
                await context.Database.ExecuteSqlRawAsync(
                    "ALTER TABLE Residents ADD COLUMN PhotoPath TEXT NULL;");
            }
            catch
            {
                // Column already exists or table created with it; safe to ignore
            }

            // Seed Roles
            string[] roles = { "Admin", "Staff" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // Seed Admin User
            var adminUser = await userManager.FindByNameAsync("admin");
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = "admin",
                    Email = "admin@menderaqoci.gov.et",
                    FullName = "Kebele Administrator",
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true,
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(adminUser, "Admin@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // Seed Staff User
            var staffUser = await userManager.FindByNameAsync("staff");
            if (staffUser == null)
            {
                staffUser = new ApplicationUser
                {
                    UserName = "staff",
                    Email = "staff@menderaqoci.gov.et",
                    FullName = "Kebele Records Staff",
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true,
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(staffUser, "Staff@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(staffUser, "Staff");
                }
            }

            // Seed SubKebeles
            if (!await context.SubKebeles.AnyAsync())
            {
                var subKebeles = new List<SubKebele>
                {
                    new SubKebele { SubKebeleName = "Sub-Kebele 01 (Central)", Description = "Central administrative zone of Mendera Qoci", IsActive = true },
                    new SubKebele { SubKebeleName = "Sub-Kebele 02 (North)", Description = "Northern residential community area", IsActive = true },
                    new SubKebele { SubKebeleName = "Sub-Kebele 03 (East)", Description = "Eastern market & agricultural zone", IsActive = true },
                    new SubKebele { SubKebeleName = "Sub-Kebele 04 (South)", Description = "Southern expanding suburb district", IsActive = true }
                };
                await context.SubKebeles.AddRangeAsync(subKebeles);
                await context.SaveChangesAsync();
            }

            // Seed Sample Residents
            if (!await context.Residents.AnyAsync())
            {
                var sub01 = await context.SubKebeles.FirstAsync(s => s.SubKebeleName.Contains("01"));
                var sub02 = await context.SubKebeles.FirstAsync(s => s.SubKebeleName.Contains("02"));
                var sub03 = await context.SubKebeles.FirstAsync(s => s.SubKebeleName.Contains("03"));

                var residents = new List<Resident>
                {
                    new Resident
                    {
                        FirstName = "Abebe",
                        LastName = "Bikila",
                        DateOfBirth = new DateTime(1985, 4, 12),
                        Gender = "Male",
                        PhoneNumber = "0911223344",
                        SubKebeleId = sub01.SubKebeleId,
                        HouseholdHead = "Abebe Bikila",
                        RegistrationDate = DateTime.Now.AddYears(-2),
                        Status = "Active"
                    },
                    new Resident
                    {
                        FirstName = "Chaltu",
                        LastName = "Gemechu",
                        DateOfBirth = new DateTime(1992, 8, 25),
                        Gender = "Female",
                        PhoneNumber = "0922334455",
                        SubKebeleId = sub02.SubKebeleId,
                        HouseholdHead = "Gemechu Daba",
                        RegistrationDate = DateTime.Now.AddYears(-1),
                        Status = "Active"
                    },
                    new Resident
                    {
                        FirstName = "Kebede",
                        LastName = "Tadesse",
                        DateOfBirth = new DateTime(1978, 11, 30),
                        Gender = "Male",
                        PhoneNumber = "0933445566",
                        SubKebeleId = sub03.SubKebeleId,
                        HouseholdHead = "Kebede Tadesse",
                        RegistrationDate = DateTime.Now.AddMonths(-8),
                        Status = "Active"
                    },
                    new Resident
                    {
                        FirstName = "Aster",
                        LastName = "Aweke",
                        DateOfBirth = new DateTime(1995, 2, 14),
                        Gender = "Female",
                        PhoneNumber = "0944556677",
                        SubKebeleId = sub01.SubKebeleId,
                        HouseholdHead = "Aweke Bekele",
                        RegistrationDate = DateTime.Now.AddMonths(-3),
                        Status = "Active"
                    }
                };

                await context.Residents.AddRangeAsync(residents);
                await context.SaveChangesAsync();

                // Seed Kebele IDs for sample residents
                var r1 = residents[0];
                var r2 = residents[1];
                var r3 = residents[2];

                var idRecords = new List<KebeleIdRecord>
                {
                    new KebeleIdRecord
                    {
                        ResidentId = r1.ResidentId,
                        IdNumber = "KID-MQ-2024-001",
                        IssueDate = DateTime.Now.AddYears(-1),
                        ExpiryDate = DateTime.Now.AddDays(15), // Expiring soon
                        IdStatus = "Active",
                        IssuedByUserId = adminUser.Id,
                        Remarks = "Initial issuance"
                    },
                    new KebeleIdRecord
                    {
                        ResidentId = r2.ResidentId,
                        IdNumber = "KID-MQ-2023-089",
                        IssueDate = DateTime.Now.AddYears(-3),
                        ExpiryDate = DateTime.Now.AddYears(-1), // Expired
                        IdStatus = "Expired",
                        IssuedByUserId = staffUser.Id,
                        Remarks = "Old ID record"
                    },
                    new KebeleIdRecord
                    {
                        ResidentId = r2.ResidentId,
                        IdNumber = "KID-MQ-2025-042",
                        IssueDate = DateTime.Now.AddMonths(-2),
                        ExpiryDate = DateTime.Now.AddYears(2), // Active renewal
                        IdStatus = "Active",
                        IssuedByUserId = staffUser.Id,
                        Remarks = "Renewed ID record"
                    },
                    new KebeleIdRecord
                    {
                        ResidentId = r3.ResidentId,
                        IdNumber = "KID-MQ-2024-112",
                        IssueDate = DateTime.Now.AddMonths(-6),
                        ExpiryDate = DateTime.Now.AddYears(1).AddMonths(6),
                        IdStatus = "Active",
                        IssuedByUserId = adminUser.Id,
                        Remarks = "Issued upon verification"
                    }
                };

                await context.KebeleIdRecords.AddRangeAsync(idRecords);

                // Seed Correspondences
                var correspondences = new List<Correspondence>
                {
                    new Correspondence
                    {
                        ResidentId = r1.ResidentId,
                        DocumentType = "Residence Certificate",
                        IssueDate = DateTime.Now.AddMonths(-5),
                        Purpose = "Bank Account Opening Requirement",
                        IssuedByUserId = adminUser.Id,
                        ReferenceNumber = "COR-MQ-2025-101"
                    },
                    new Correspondence
                    {
                        ResidentId = r2.ResidentId,
                        DocumentType = "Support Letter",
                        IssueDate = DateTime.Now.AddMonths(-1),
                        Purpose = "Higher Education Scholarship Application",
                        IssuedByUserId = staffUser.Id,
                        ReferenceNumber = "COR-MQ-2025-204"
                    }
                };

                await context.Correspondences.AddRangeAsync(correspondences);
                await context.SaveChangesAsync();
            }
        }
    }
}
