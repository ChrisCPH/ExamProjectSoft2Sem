using Microsoft.EntityFrameworkCore;
using SearchService.Models;

namespace SearchService.Data
{
    public class SearchDbContext : DbContext 
    {
        public SearchDbContext(DbContextOptions<SearchDbContext> options) : base(options) {}

        public DbSet<Restaurant> Restaurant { get; set; } = null!;
        public DbSet<MenuItem> MenuItem { get; set; } = null!;
        public DbSet<Categories> Categories { get; set; } = null!;
    }
}
