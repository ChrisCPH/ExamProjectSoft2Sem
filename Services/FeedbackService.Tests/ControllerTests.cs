using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using FeedbackService.Services;
using FeedbackService.Controllers;
using FeedbackService.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
public class FeedbackControllerTests
{
    private readonly FeedbackController _controller;
    private readonly Mock<IFeedbackService> _mockService;

    public FeedbackControllerTests()
    {
        _mockService = new Mock<IFeedbackService>();
        _controller = new FeedbackController(_mockService.Object);
    }

    [Fact]
    public async Task CreateFeedback_NullFeedback_ReturnsBadRequest()
    {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        var result = await _controller.CreateFeedback(null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task CreateFeedback_ValidFeedback_ReturnsOk()
    {
        var feedback = new Feedback
        {
            FoodRating = 4,
            DeliveryRating = 5,
            Comment = "Test Comment",
            RestaurantID = 1,
            DeliveryDriverID = 2
        };

        var feedbackCreated = new Feedback
        {
            FeedbackID = 1,
            FoodRating = feedback.FoodRating,
            DeliveryRating = feedback.DeliveryRating,
            Comment = feedback.Comment,
            RestaurantID = feedback.RestaurantID,
            DeliveryDriverID = feedback.DeliveryDriverID
        };

        _mockService.Setup(s => s.CreateFeedback(feedback))
                    .ReturnsAsync(feedbackCreated);

        var result = await _controller.CreateFeedback(feedback);

        Assert.NotNull(result);
        Assert.IsType<OkObjectResult>(result);
    }


    [Fact]
    public async Task GetAverageFoodRatingById_ValidId_ReturnsOkWithRating()
    {
        _mockService.Setup(s => s.CalculateAverageFoodRating(It.IsAny<int>()))
                    .ReturnsAsync(4.5);

        var result = await _controller.GetAverageFoodRatingById(1);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(4.5, okResult.Value);
    }

    [Fact]
    public async Task GetAverageDeliveryRatingById_ValidId_ReturnsOkWithRating()
    {
        _mockService.Setup(s => s.CalculateAverageDeliveryRating(It.IsAny<int>()))
                    .ReturnsAsync(4.2);

        var result = await _controller.GetAverageDeliveryRatingById(1);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(4.2, okResult.Value);
    }
}
