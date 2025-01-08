using Xunit;
using Moq;
using FeedbackService.Repositories;
using FeedbackService.Models;
using FeedbackService.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class FeedbackServiceTests
{
    private readonly FeedbackService.Services.FeedbackService _service;
    private readonly Mock<IFeedbackRepository> _mockRepo;

    public FeedbackServiceTests()
    {
        _mockRepo = new Mock<IFeedbackRepository>();
        _service = new FeedbackService.Services.FeedbackService(_mockRepo.Object);
    }

    [Fact]
    public async Task CalculateAverageFoodRating_NoFeedback_ReturnsZero()
    {
        _mockRepo.Setup(r => r.GetFeedbackByRestaurantId(It.IsAny<int>()))
                 .ReturnsAsync(new List<Feedback>());

        var result = await _service.CalculateAverageFoodRating(1);

        Assert.Equal(0, result);
    }

    [Fact]
    public async Task CalculateAverageFoodRating_WithFeedback_ReturnsAverage()
    {
        var feedbacks = new List<Feedback>
        {
            new Feedback { FoodRating = 4 },
            new Feedback { FoodRating = 5 }
        };

        _mockRepo.Setup(r => r.GetFeedbackByRestaurantId(It.IsAny<int>()))
                 .ReturnsAsync(feedbacks);

        var result = await _service.CalculateAverageFoodRating(1);

        Assert.Equal(4.5, result);
    }

    [Fact]
    public async Task CreateFeedback_ValidFeedback_CallsRepository()
    {
        var feedback = new Feedback { FoodRating = 5, DeliveryRating = 5 };
        _mockRepo.Setup(r => r.AddFeedback(feedback))
                 .ReturnsAsync(feedback);

        await _service.CreateFeedback(feedback);

        _mockRepo.Verify(r => r.AddFeedback(feedback), Times.Once);
    }
}
