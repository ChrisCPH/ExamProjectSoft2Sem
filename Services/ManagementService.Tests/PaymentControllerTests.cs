using ManagementService.Controllers;
using ManagementService.Models;
using ManagementService.Services;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Xunit;

public class PaymentControllerTests
{
    private readonly Mock<IPaymentService> _mockPaymentService;
    private readonly PaymentController _controller;

    public PaymentControllerTests()
    {
        _mockPaymentService = new Mock<IPaymentService>();
        _controller = new PaymentController(_mockPaymentService.Object);
    }

    [Fact]
    public async Task GetAllPayments_ValidRequest_ReturnsOk()
    {
        var mockPayments = new List<Payment>
    {
        new Payment { PaymentID = 1, DeliveryDriverID = 101, Amount = 100.50m, Status = "Pending" },
        new Payment { PaymentID = 2, DeliveryDriverID = 102, Amount = 200.75m, Status = "Paid" }
    };

        _mockPaymentService.Setup(s => s.GetAllPayments())
            .ReturnsAsync(mockPayments);

        var result = await _controller.GetAllPayments();

        Assert.NotNull(result);
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetPaymentById_ValidId_ReturnsOk()
    {
        var paymentId = 1;
        var mockPayment = new Payment
        {
            PaymentID = paymentId,
            DeliveryDriverID = 101,
            Amount = 100.50m,
            Status = "Pending"
        };

        _mockPaymentService.Setup(s => s.GetPaymentById(paymentId))
            .ReturnsAsync(mockPayment);

        var result = await _controller.GetPaymentById(paymentId);

        Assert.NotNull(result);
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task CalculatePayment_ValidDriverId_ReturnsCreatedAtAction()
    {
        var driverId = 101;
        var dateRequest = new DateRequest
        {
            StartDate = DateTime.Now.AddDays(-7),
            EndDate = DateTime.Now
        };
        var mockPayment = new Payment
        {
            PaymentID = 1,
            DeliveryDriverID = driverId,
            Amount = 150.00m,
            Status = "Pending"
        };

        _mockPaymentService.Setup(s => s.CalculateAndSavePaymentAsync(driverId, dateRequest.StartDate, dateRequest.EndDate))
            .ReturnsAsync(mockPayment);

        var result = await _controller.CalculatePayment(driverId, dateRequest);

        Assert.NotNull(result);
        Assert.IsType<CreatedAtActionResult>(result);
    }

    [Fact]
    public async Task UpdatePaymentStatus_ValidPaymentId_ReturnsOk()
    {
        var paymentId = 1;

        _mockPaymentService.Setup(s => s.UpdatePaymentStatus(paymentId))
            .ReturnsAsync(true);

        var result = await _controller.UpdatePaymentStatus(paymentId);

        Assert.NotNull(result);
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task UpdatePaymentStatus_InvalidPaymentId_ReturnsNotFound()
    {
        var paymentId = 1;

        _mockPaymentService.Setup(s => s.UpdatePaymentStatus(paymentId))
            .ReturnsAsync(false);

        var result = await _controller.UpdatePaymentStatus(paymentId);

        Assert.NotNull(result);
        Assert.IsType<NotFoundObjectResult>(result);
    }
}
