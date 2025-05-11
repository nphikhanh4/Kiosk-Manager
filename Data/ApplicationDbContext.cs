using Microsoft.EntityFrameworkCore;
using KioskManagementWebApp.Models;

namespace KioskManagementWebApp.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Document> Documents { get; set; }
        public DbSet<Kiosk> Kiosks { get; set; }
        public DbSet<Renter> Renters { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<TuitionFee> TuitionFees { get; set; }
        public DbSet<TuitionFeeDetail> TuitionFeeDetails { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Cấu hình mối quan hệ 1-nhiều giữa Kiosk và Renter
            modelBuilder.Entity<Kiosk>()
                .HasMany(k => k.Renters)
                .WithOne(r => r.Kiosk)
                .HasForeignKey(r => r.KioskId)
                .OnDelete(DeleteBehavior.Cascade);

            // Cấu hình mối quan hệ 1-nhiều giữa Renter và Payment
            modelBuilder.Entity<Renter>()
                .HasMany(r => r.Payments)
                .WithOne(p => p.Renter)
                .HasForeignKey(p => p.RenterId)
                .OnDelete(DeleteBehavior.Cascade);

            // Cấu hình mối quan hệ 1-nhiều giữa Student và TuitionFee
            modelBuilder.Entity<Student>()
                .HasMany(s => s.TuitionFees)
                .WithOne(tf => tf.Student)
                .HasForeignKey(tf => tf.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            // Cấu hình mối quan hệ 1-nhiều giữa TuitionFee và TuitionFeeDetail
            modelBuilder.Entity<TuitionFee>()
                .HasMany(tf => tf.TuitionFeeDetails)
                .WithOne(tfd => tfd.TuitionFee)
                .HasForeignKey(tfd => tfd.TuitionFeeId)
                .OnDelete(DeleteBehavior.Cascade);

            base.OnModelCreating(modelBuilder);
        }
    }
}