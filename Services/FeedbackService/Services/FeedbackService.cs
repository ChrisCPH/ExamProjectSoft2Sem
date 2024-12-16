using FeedbackService.Repositories;
using FeedbackService.Models;

namespace FeedbackService.Services
{
    public interface IFeedbackService
    {
        Task<Feedback> CreateFeedback(Feedback feedback);
        Task<double> CalculateAverageFoodRating(int restaurantId);
        Task<double> CalculateAverageDeliveryRating(int restaurantId);
    }

    public class FeedbackService : IFeedbackService
    {
        private readonly IFeedbackRepository _repository;

        public FeedbackService(IFeedbackRepository feedbackRepository)
        {
            _repository = feedbackRepository;
        }

        public async Task<double> CalculateAverageFoodRating(int restaurantId)
        {
            var feedbacks = await _repository.GetFeedbackByRestaurantId(restaurantId);

            if (!feedbacks.Any())
                return 0;

            return

             feedbacks.Average(f => f.FoodRating);
        }

        public async Task<double> CalculateAverageDeliveryRating(int driverId)
        {
            var feedbacks = await _repository.GetFeedbackByDeliveryDriverId(driverId);

            if (!feedbacks.Any())
                return 0;

            return feedbacks.Average(f => f.DeliveryRating);
        }

        public async Task<Feedback> CreateFeedback(Feedback feedback)
        {
            var feedbackCreated = await _repository.AddFeedback(feedback);
            return feedbackCreated;
        }
    }
}