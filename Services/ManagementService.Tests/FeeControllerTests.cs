using ManagementService.Controllers;
using ManagementService.Models;
using ManagementService.Services;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Xunit;

public class FeeControllerTests
{
    private readonly Mock<IFeeService> _mockFeeService;
    private readonly FeeController _controller;

    public FeeControllerTests()
    {
        _mockFeeService = new Mock<IFeeService>();
        _controller = new FeeController(_mockFeeService.Object);
    }

    [Fact]
    public async Task GetAllFees_ValidRequest_ReturnsOk()
    {
        var mockFees = new List<Fee>
    {
        new Fee { FeeID = 1, RestaurantID = 101, Amount = 50.00m, Status = "Pending" },
        new Fee { FeeID = 2, RestaurantID = 102, Amount = 75.50m, Status = "Paid" }
    };

        _mockFeeService.Setup(s => s.GetAllFees())
            .ReturnsAsync(mockFees);

        var result = await _controller.GetAllFees();

        Assert.NotNull(result);
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetFeeById_ValidId_ReturnsOk()
    {
        var feeId = 1;
        var mockFee = new Fee
        {
            FeeID = feeId,
            RestaurantID = 101,
            Amount = 50.00m,
            Status = "Pending"
        };

        _mockFeeService.Setup(s => s.GetFeeById(feeId))
            .ReturnsAsync(mockFee);

        var result = await _controller.GetFeeById(feeId);

        Assert.NotNull(result);
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task CalculateFee_ValidRestaurantId_ReturnsCreatedAtAction()
    {
        var restaurantId = 101;
        var dateRequest = new DateRequest
        {
            StartDate = DateTime.Now.AddDays(-7),
            EndDate = DateTime.Now
        };
        var mockFee = new Fee
        {
            FeeID = 1,
            RestaurantID = restaurantId,
            Amount = 50.00m,
            Status = "Pending"
        };

        _mockFeeService.Setup(s => s.CalculateAndSaveFeeAsync(restaurantId, dateRequest.StartDate, dateRequest.EndDate))
            .ReturnsAsync(mockFee);

        var result = await _controller.CalculateFee(restaurantId, dateRequest);

        Assert.NotNull(result);
        Assert.IsType<CreatedAtActionResult>(result);
    }

    [Fact]
    public async Task UpdateFeeStatus_ValidFeeId_ReturnsOk()
    {
        var feeId = 1;

        _mockFeeService.Setup(s => s.UpdateFeeStatus(feeId))
            .ReturnsAsync(true);

        var result = await _controller.UpdateFeeStatus(feeId);

        Assert.NotNull(result);
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task UpdateFeeStatus_InvalidFeeId_ReturnsNotFound()
    {
        var feeId = 1;

        _mockFeeService.Setup(s => s.UpdateFeeStatus(feeId))
            .ReturnsAsync(false);

        var result = await _controller.UpdateFeeStatus(feeId);

        Assert.NotNull(result);
        Assert.IsType<NotFoundObjectResult>(result);
    }

}