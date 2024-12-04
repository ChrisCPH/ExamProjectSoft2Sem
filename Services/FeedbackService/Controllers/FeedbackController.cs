using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using FeedbackService.Services;
using FeedbackService.Models;

namespace FeedbackService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FeedbackController : ControllerBase
    {
        private readonly IFeedbackService _feedbackService;

        public FeedbackController(IFeedbackService feedbackService)
        {
            _feedbackService = feedbackService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateFeedback([FromBody] Feedback feedback)
        {
            if (feedback == null)
            {
                return BadRequest("Feedback cannot be null.");
            }

            try
            {
                var feedbackCreated = await _feedbackService.CreateFeedback(feedback);
                return Ok(new { message = "Feedback created", feedback = feedbackCreated });
            }
            catch (Exception ex)
            {

                return StatusCode(500, new { Message = "An error occurred.", Details = ex.Message });
            }
        }

        [HttpGet("avgFoodRating/{id}")]
        public async Task<IActionResult> GetAverageFoodRatingById(int id)
        {
            var avgRating = await _feedbackService.CalculateAverageFoodRating(id);

            return Ok(avgRating);
        }

        [HttpGet("avgDeliveryRating/{id}")]
        public async Task<IActionResult> GetAverageDeliveryRatingById(int id)
        {
            var avgRating = await _feedbackService.CalculateAverageDeliveryRating(id);

            return Ok(avgRating);
        }
    }
}
