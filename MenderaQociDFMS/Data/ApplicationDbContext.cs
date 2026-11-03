using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MenderaQociDFMS.Models;

namespace MenderaQociDFMS.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<SubKebele> SubKebeles { get; set; } = null!;
        public DbSet<Resident> Residents { get; set; } = null!;
        public DbSet<KebeleIdRecord> KebeleIdRecords { get; set; } = null!;
        public DbSet<Correspondence> Correspondences { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // SubKebele -> Resident (One-to-Many, Restrict Delete)
            builder.Entity<Resident>()
                .HasOne(r => r.SubKebele)
                .WithMany(s => s.Residents)
                .HasForeignKey(r => r.SubKebeleId)
                .OnDelete(DeleteBehavior.Restrict);

            // Resident -> KebeleIdRecord (One-to-Many, Cascade Delete)
            builder.Entity<KebeleIdRecord>()
                .HasOne(k => k.Resident)
                .WithMany(r => r.KebeleIdRecords)
                .HasForeignKey(k => k.ResidentId)
                .OnDelete(DeleteBehavior.Cascade);

            // Resident -> Correspondence (One-to-Many, Cascade Delete)
            builder.Entity<Correspondence>()
                .HasOne(c => c.Resident)
                .WithMany(r => r.Correspondences)
                .HasForeignKey(c => c.ResidentId)
                .OnDelete(DeleteBehavior.Cascade);

            // Unique constraint on Kebele ID Number
            builder.Entity<KebeleIdRecord>()
                .HasIndex(k => k.IdNumber)
                .IsUnique();

            // Unique constraint on Correspondence Reference Number
            builder.Entity<Correspondence>()
                .HasIndex(c => c.ReferenceNumber)
                .IsUnique();
        }
    }
}
