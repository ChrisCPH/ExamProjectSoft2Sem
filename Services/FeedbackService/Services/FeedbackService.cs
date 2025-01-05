using FeedbackService.Repositories;
using FeedbackService.Models;
using System.Text.Encodings.Web;

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
            if (!string.IsNullOrEmpty(feedback.Comment))
            {
                feedback.Comment = InputSanitizer.Sanitize(feedback.Comment);
            }

            var feedbackCreated = await _repository.AddFeedback(feedback);
            return feedbackCreated;
        }
    }

    public static class InputSanitizer
    {
        public static string Sanitize(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            return HtmlEncoder.Default.Encode(input.Trim());
        }
    }
}