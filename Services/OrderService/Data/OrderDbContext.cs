using Microsoft.EntityFrameworkCore;
using OrderService.Models;

namespace OrderService.Data
{
    public class OrderDbContext : DbContext
    {
        public OrderDbContext(DbContextOptions<OrderDbContext> options): base(options){ }
 
        public DbSet<Order> Order { get; set; } = null!;
        public DbSet<OrderItem> OrderItem { get; set; } = null!;
    }
}
