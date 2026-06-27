using Employee_Verification_System.Admin.Models;
using Microsoft.EntityFrameworkCore;

namespace Employee_Verification_System.Admin.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // Use fully qualified name to avoid conflict
        public DbSet<Models.Admin> Admins { get; set; }
        public DbSet<Verification> Verifications { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<FraudDetection> FraudDetections { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Admin configuration - use fully qualified name
            modelBuilder.Entity<Models.Admin>(entity =>
            {
                entity.HasKey(e => e.AdminID);
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Email).IsRequired();
                entity.Property(e => e.Role).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Department).IsRequired().HasMaxLength(100);

                // Index for email
                entity.HasIndex(e => e.Email).IsUnique();
            });

            // Verification configuration
            modelBuilder.Entity<Verification>(entity =>
            {
                entity.HasKey(e => e.VerificationID);
                entity.Property(e => e.EmployeeEmail).IsRequired();
                entity.Property(e => e.VerificationType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Notes).HasMaxLength(500);

                // Relationships - use fully qualified name
                entity.HasOne(v => v.Admin)
                      .WithMany(a => a.Verifications)
                      .HasForeignKey(v => v.AdminID)
                      .OnDelete(DeleteBehavior.Restrict);

                // Index for employee email and status
                entity.HasIndex(v => v.EmployeeEmail);
                entity.HasIndex(v => v.Status);
                entity.HasIndex(v => v.VerificationDate);
            });

            // Report configuration
            modelBuilder.Entity<Report>(entity =>
            {
                entity.HasKey(e => e.ReportID);
                entity.Property(e => e.ReportData).HasMaxLength(500);

                // Relationships - use fully qualified name
                entity.HasOne(r => r.Admin)
                      .WithMany(a => a.Reports)
                      .HasForeignKey(r => r.AdminID)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Notification configuration
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasKey(e => e.NotificationID);
                entity.Property(e => e.Message).IsRequired();
                entity.Property(e => e.Channel).IsRequired().HasMaxLength(20);
            });

            // FraudDetection configuration
            modelBuilder.Entity<FraudDetection>(entity =>
            {
                entity.HasKey(e => e.DetectionID);
                entity.Property(e => e.EmployeeID).IsRequired();
                entity.Property(e => e.SuspicionReason).IsRequired();
                entity.Property(e => e.ActionTaken).HasMaxLength(200);
            });

            // Seed initial admin data - use fully qualified name
            modelBuilder.Entity<Models.Admin>().HasData(
                new Models.Admin
                {
                    AdminID = 1,
                    FirstName = "System",
                    LastName = "Administrator",
                    Email = "admin@company.com",
                    Role = "Super Admin",
                    Department = "IT"
                }
            );
        }
    }
}