using ManagementService.Models;
using Microsoft.EntityFrameworkCore;

namespace ManagementService.Data
{
    public class ManagementDbContext : DbContext
    {
        public ManagementDbContext(DbContextOptions<ManagementDbContext> options) : base(options) { }
    
        public DbSet<Fee> Fee { get; set; } = null!;

        public DbSet<Payment> Payment { get; set; } = null!;
    }
}
