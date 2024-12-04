using FeedbackService.Data;
using FeedbackService.Models;
using Microsoft.EntityFrameworkCore;

namespace FeedbackService.Repositories
{
    public interface IFeedbackRepository
    {
        Task<Feedback> AddFeedback(Feedback feedback);
        Task<IEnumerable<Feedback>> GetFeedbackByRestaurantId(int restaurantId);
        Task<IEnumerable<Feedback>> GetFeedbackByDeliveryDriverId(int deliveryDriverId);
    }

    public class FeedbackRepository : IFeedbackRepository
    {
        private readonly FeedbackDbContext _context;

        public FeedbackRepository(FeedbackDbContext context)
        {
            _context = context;
        }

        public async Task<Feedback> AddFeedback(Feedback feedback)
        {
            _context.Feedback.Add(feedback);
            await _context.SaveChangesAsync();
            return feedback;
        }

        public async Task<IEnumerable<Feedback>> GetFeedbackByRestaurantId(int restaurantId)
        {
            return await _context.Feedback
                .Where(f => f.RestaurantID == restaurantId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Feedback>> GetFeedbackByDeliveryDriverId(int deliveryDriverId)
        {
            return await _context.Feedback
                .Where(f => f.DeliveryDriverID == deliveryDriverId)
                .ToListAsync();
        }
    }
}