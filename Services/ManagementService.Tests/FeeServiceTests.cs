using ManagementService.Models;
using ManagementService.Repositories;
using ManagementService.Services;
using Moq;
using Moq.Protected;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Xunit;

public class FeeServiceTests
{
    [Fact]
    public async Task GetAllFees_ShouldReturnAllFees()
    {
        var mockRepository = new Mock<IFeeRepository>();
        mockRepository.Setup(repo => repo.GetAllFees())
            .ReturnsAsync(new List<Fee>
            {
                new Fee { FeeID = 1, Amount = 50.00m, RestaurantID = 101, Status = "Pending" },
                new Fee { FeeID = 2, Amount = 75.00m, RestaurantID = 102, Status = "Paid" }
            });

        var feeService = new FeeService(mockRepository.Object, new HttpClient());

        var result = await feeService.GetAllFees();

        Assert.Equal(2, result.Count);
        Assert.Contains(result, f => f.FeeID == 1 && f.Amount == 50.00m);
        Assert.Contains(result, f => f.FeeID == 2 && f.Status == "Paid");
    }

    [Fact]
    public async Task GetFeeById_ShouldReturnCorrectFee()
    {
        var mockRepository = new Mock<IFeeRepository>();
        mockRepository.Setup(repo => repo.GetFeeById(1))
            .ReturnsAsync(new Fee { FeeID = 1, Amount = 50.00m, RestaurantID = 101, Status = "Pending" });

        var feeService = new FeeService(mockRepository.Object, new HttpClient());

        var result = await feeService.GetFeeById(1);

        Assert.NotNull(result);
        Assert.Equal(50.00m, result?.Amount);
        Assert.Equal("Pending", result?.Status);
    }

    [Fact]
    public async Task CalculateAndSaveFeeAsync_ShouldCalculateFeeAndSaveToRepository()
    {
        var mockRepository = new Mock<IFeeRepository>();
        mockRepository.Setup(repo => repo.AddFee(It.IsAny<Fee>())).Returns(Task.CompletedTask);

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

                return new HttpResponseMessage(HttpStatusCode.NotFound);
            });

        var mockHttpClient = new HttpClient(mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("http://localhost:5149")
        };

        var feeService = new FeeService(mockRepository.Object, mockHttpClient);

        var fee = await feeService.CalculateAndSaveFeeAsync(101, DateTime.Now.AddDays(-7), DateTime.Now);

        Assert.NotNull(fee);
        Assert.Equal(101, fee.RestaurantID);
        Assert.Equal(20, fee.OrderCount);
        Assert.Equal(1000.00m, fee.TotalOrderPrice);
        mockRepository.Verify(repo => repo.AddFee(It.IsAny<Fee>()), Times.Once);
    }

    [Fact]
    public async Task GetTotalOrderPrice_ShouldReturnCorrectTotalPriceFromApi()
    {
        var mockRepository = new Mock<IFeeRepository>();

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
                        Content = new StringContent("1000.50")
                    };
                }

                return new HttpResponseMessage(HttpStatusCode.NotFound);
            });

        var mockHttpClient = new HttpClient(mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("http://localhost:5149")
        };

        var feeService = new FeeService(mockRepository.Object, mockHttpClient);

        var totalOrderPrice = await feeService.GetTotalOrderPrice(101, DateTime.Now.AddDays(-7), DateTime.Now);

        Assert.Equal(1000.50m, totalOrderPrice);
    }

    [Fact]
    public async Task GetOrderCount_ShouldReturnCorrectCountFromApi()
    {
        var mockRepository = new Mock<IFeeRepository>();

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
                        Content = new StringContent("25")
                    };
                }

                return new HttpResponseMessage(HttpStatusCode.NotFound);
            });

        var mockHttpClient = new HttpClient(mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("http://localhost:5149")
        };

        var feeService = new FeeService(mockRepository.Object, mockHttpClient);

        var orderCount = await feeService.GetOrderCount(101, DateTime.Now.AddDays(-7), DateTime.Now);

        Assert.Equal(25, orderCount);
    }

    [Fact]
    public async Task UpdateFeeStatus_ShouldUpdateStatusAndReturnTrue()
    {
        var mockRepository = new Mock<IFeeRepository>();
        var existingFee = new Fee
        {
            FeeID = 1,
            Status = "Pending",
            PaidDate = new DateTime(2000, 1, 1)
        };

        mockRepository.Setup(repo => repo.GetFeeById(1)).ReturnsAsync(existingFee);
        mockRepository.Setup(repo => repo.UpdateFee(It.IsAny<Fee>())).Returns(Task.CompletedTask);

        var feeService = new FeeService(mockRepository.Object, new HttpClient());

        var result = await feeService.UpdateFeeStatus(1);

        Assert.True(result);
        Assert.Equal("Paid", existingFee.Status);
        Assert.NotEqual(new DateTime(2000, 1, 1), existingFee.PaidDate);
        mockRepository.Verify(repo => repo.UpdateFee(It.Is<Fee>(f => f.Status == "Paid")), Times.Once);
    }

    [Fact]
    public async Task UpdateFeeStatus_ShouldReturnFalseIfFeeDoesNotExist()
    {
        var mockRepository = new Mock<IFeeRepository>();
        mockRepository.Setup(repo => repo.GetFeeById(It.IsAny<int>())).ReturnsAsync((Fee?)null);

        var feeService = new FeeService(mockRepository.Object, new HttpClient());

        var result = await feeService.UpdateFeeStatus(1);

        Assert.False(result);
        mockRepository.Verify(repo => repo.UpdateFee(It.IsAny<Fee>()), Times.Never);
    }
}
