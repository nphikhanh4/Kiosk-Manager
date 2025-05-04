using Microsoft.EntityFrameworkCore;
using KioskManagementWebApp.Models;
using System.Collections.Generic;

namespace KioskManagementWebApp.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Kiosk> Kiosks { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<Tuition> Tuitions { get; set; }
    }
}