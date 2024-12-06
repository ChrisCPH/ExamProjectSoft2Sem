using Moq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Moq.Protected;
using Xunit;
using ManagementService.Models;
using ManagementService.Repositories;
using ManagementService.Services;

public class PaymentServiceTests
{

    [Fact]
    public async Task GetAllPayments_ShouldReturnListOfPayments()
    {
        var mockPaymentRepository = new Mock<IPaymentRepository>();

        var mockPayments = new List<Payment>
        {
            new Payment { PaymentID = 1, DeliveryDriverID = 101, Amount = 100.50m, Status = "Pending" },
            new Payment { PaymentID = 2, DeliveryDriverID = 102, Amount = 200.75m, Status = "Paid" }
        };

        mockPaymentRepository
            .Setup(repo => repo.GetAllPayments())
            .ReturnsAsync(mockPayments);

        var paymentService = new PaymentService(mockPaymentRepository.Object, new HttpClient());

        var payments = await paymentService.GetAllPayments();

        Assert.NotNull(payments);
        Assert.Equal(2, payments.Count);
        Assert.Equal(100.50m, payments[0].Amount);
    }

    [Fact]
    public async Task GetPaymentById_ShouldReturnCorrectPayment()
    {
        var mockPaymentRepository = new Mock<IPaymentRepository>();

        var mockPayment = new Payment
        {
            PaymentID = 1,
            DeliveryDriverID = 101,
            Amount = 100.50m,
            Status = "Pending"
        };

        mockPaymentRepository
            .Setup(repo => repo.GetPaymentById(1))
            .ReturnsAsync(mockPayment);

        var paymentService = new PaymentService(mockPaymentRepository.Object, new HttpClient());

        var payment = await paymentService.GetPaymentById(1);

        Assert.NotNull(payment);
        Assert.Equal(1, payment.PaymentID);
        Assert.Equal(100.50m, payment.Amount);
        Assert.Equal("Pending", payment.Status);
    }

    [Fact]
    public async Task CalculateAndSavePaymentAsync_ShouldCalculatePaymentAndSaveToRepository()
    {
        var mockRepository = new Mock<IPaymentRepository>();
        mockRepository.Setup(repo => repo.AddPayment(It.IsAny<Payment>())).Returns(Task.CompletedTask);

        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();

        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync((HttpRequestMessage request, CancellationToken _) =>
            {
                if (request.RequestUri?.ToString().Contains("totalprice") == true)
                {
                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent("1000.00")
                    };
                }

                if (request.RequestUri?.ToString().Contains("ordercount") == true)
                {
                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent("20")
                    };
                }

                if (request.RequestUri?.ToString().Contains("avgDeliveryRating") == true)
                {
                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent("4.5")
                    };
                }

                return new HttpResponseMessage(HttpStatusCode.NotFound);
            });

        var mockHttpClient = new HttpClient(mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("http://localhost:5149")
        };

        var paymentService = new PaymentService(mockRepository.Object, mockHttpClient);

        var payment = await paymentService.CalculateAndSavePaymentAsync(101, DateTime.Now.AddDays(-7), DateTime.Now);

        Assert.NotNull(payment);
        Assert.Equal(101, payment.DeliveryDriverID);
        Assert.Equal(20, payment.DeliveryCount);
        Assert.Equal(1000.00m, payment.TotalDeliveryPrice);
        Assert.Equal("Pending", payment.Status);
        mockRepository.Verify(repo => repo.AddPayment(It.IsAny<Payment>()), Times.Once);
    }

    [Fact]
    public async Task GetAvgRating_ShouldReturnAvgRatingFromApi()
    {
        var mockRepository = new Mock<IPaymentRepository>();
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();

        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync((HttpRequestMessage request, CancellationToken _) =>
            {
                if (request.RequestUri?.ToString().Contains("avgDeliveryRating") == true)
                {
                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent("4.5")
                    };
                }

                return new HttpResponseMessage(HttpStatusCode.NotFound);
            });

        var mockHttpClient = new HttpClient(mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("http://localhost:5193")
        };

        var paymentService = new PaymentService(mockRepository.Object, mockHttpClient);

        var avgRating = await paymentService.GetAvgRating(101);

        Assert.Equal(4.5, avgRating);
    }

    [Fact]
    public async Task GetTotalDeliveryPrice_ShouldReturnCorrectTotalPriceFromApi()
    {
        var mockRepository = new Mock<IPaymentRepository>();
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();

        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync((HttpRequestMessage request, CancellationToken _) =>
            {
                if (request.RequestUri?.ToString().Contains("totalprice") == true)
                {
                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent("1000.00")
                    };
                }

                return new HttpResponseMessage(HttpStatusCode.NotFound);
            });

        var mockHttpClient = new HttpClient(mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("http://localhost:5149")
        };

        var paymentService = new PaymentService(mockRepository.Object, mockHttpClient);

        var totalDeliveryPrice = await paymentService.GetTotalDeliveryPrice(101, DateTime.Now.AddDays(-7), DateTime.Now);

        Assert.Equal(1000.00m, totalDeliveryPrice);
    }

    [Fact]
    public async Task GetDeliveryCount_ShouldReturnCorrectCountFromApi()
    {
        var mockRepository = new Mock<IPaymentRepository>();

        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();

        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync((HttpRequestMessage request, CancellationToken _) =>
            {
                if (request.RequestUri?.ToString().Contains("ordercount") == true)
                {
                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent("15")
                    };
                }

                return new HttpResponseMessage(HttpStatusCode.NotFound);
            });

        var mockHttpClient = new HttpClient(mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("http://localhost:5149")
        };

        var paymentService = new PaymentService(mockRepository.Object, mockHttpClient);

        var deliveryCount = await paymentService.GetDeliveryCount(101, DateTime.Now.AddDays(-7), DateTime.Now);

        Assert.Equal(15, deliveryCount);
    }

    [Fact]
    public async Task UpdatePaymentStatus_ShouldUpdateStatusToPaid()
    {
        var mockRepository = new Mock<IPaymentRepository>();
        mockRepository.Setup(repo => repo.GetPaymentById(It.IsAny<int>())).ReturnsAsync(new Payment
        {
            PaymentID = 1,
            Status = "Pending"
        });
        mockRepository.Setup(repo => repo.UpdatePayment(It.IsAny<Payment>())).Returns(Task.CompletedTask);

        var mockHttpClient = new Mock<HttpClient>();

        var paymentService = new PaymentService(mockRepository.Object, mockHttpClient.Object);

        var result = await paymentService.UpdatePaymentStatus(1);

        Assert.True(result);
        mockRepository.Verify(repo => repo.UpdatePayment(It.IsAny<Payment>()), Times.Once);
    }

    [Fact]
    public async Task UpdatePaymentStatus_ShouldReturnFalseWhenPaymentNotFound()
    {
        var mockRepository = new Mock<IPaymentRepository>();
        mockRepository.Setup(repo => repo.GetPaymentById(It.IsAny<int>())).ReturnsAsync((Payment?)null);

        var mockHttpClient = new Mock<HttpClient>();

        var paymentService = new PaymentService(mockRepository.Object, mockHttpClient.Object);

        var result = await paymentService.UpdatePaymentStatus(999);

        Assert.False(result);
        mockRepository.Verify(repo => repo.UpdatePayment(It.IsAny<Payment>()), Times.Never);
    }
}
