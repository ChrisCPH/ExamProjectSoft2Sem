using Microsoft.EntityFrameworkCore;
using FeedbackService.Models;
using FeedbackService.Repositories;
using FeedbackService.Data;

public class FeedbackRepositoryTests
{
    private FeedbackDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<FeedbackDbContext>()
            .UseInMemoryDatabase("FeedbackTestDb")
            .Options;

        return new FeedbackDbContext(options);
    }

    [Fact]
    public async Task AddFeedback_AddsFeedback_CallsSaveChanges()
    {
        var feedback = new Feedback { FoodRating = 4, DeliveryRating = 5, RestaurantID = 1, DeliveryDriverID = 2 };

        using var context = CreateInMemoryContext();
        var repository = new FeedbackRepository(context);

        await repository.AddFeedback(feedback);

        var addedFeedback = context.Feedback.FirstOrDefault(f => f.FeedbackID == feedback.FeedbackID);

        Assert.NotNull(addedFeedback);
        Assert.Equal(4, addedFeedback.FoodRating);
        Assert.Equal(5, addedFeedback.DeliveryRating);
    }

    [Fact]
    public async Task GetFeedbackByRestaurantId_ReturnsFeedback_ForRestaurant()
    {
        var feedbacks = new[]
        {
            new Feedback { RestaurantID = 1, FoodRating = 4, DeliveryRating = 5 },
            new Feedback { RestaurantID = 1, FoodRating = 3, DeliveryRating = 4 }
        };

        using var context = CreateInMemoryContext();
        context.Feedback.AddRange(feedbacks);
        await context.SaveChangesAsync();

        var repository = new FeedbackRepository(context);

        var result = await repository.GetFeedbackByRestaurantId(1);

        Assert.Equal(2, result.Count());
        Assert.All(result, f => Assert.Equal(1, f.RestaurantID));
    }

    [Fact]
    public async Task GetFeedbackByDeliveryDriverId_ReturnsFeedback_ForDeliveryDriver()
    {
        var feedbacks = new[]
        {
            new Feedback { DeliveryDriverID = 1, FoodRating = 4, DeliveryRating = 5 },
            new Feedback { DeliveryDriverID = 1, FoodRating = 3, DeliveryRating = 4 }
        };

        using var context = CreateInMemoryContext();
        context.Feedback.AddRange(feedbacks);
        await context.SaveChangesAsync();

        var repository = new FeedbackRepository(context);

        var result = await repository.GetFeedbackByDeliveryDriverId(1);

        Assert.Equal(2, result.Count());
        Assert.All(result, f => Assert.Equal(1, f.DeliveryDriverID));
    }
}
