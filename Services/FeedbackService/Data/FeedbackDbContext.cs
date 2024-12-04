using Microsoft.EntityFrameworkCore;
using FeedbackService.Models;

namespace FeedbackService.Data
{
    public class FeedbackDbContext : DbContext
    {
        public FeedbackDbContext(DbContextOptions<FeedbackDbContext> options): base(options){ }
 
        public DbSet<Feedback> Feedback { get; set; } = null!;
    }
}
