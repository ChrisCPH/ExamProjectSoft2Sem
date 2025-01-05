using FluentAssertions;
using ManagementService.Models;
using ManagementService.Repositories;
using ManagementService.Services;
using FeedbackService.Models;
using FeedbackService.Repositories;
using FeedbackService.Services;
using Moq;
using Newtonsoft.Json;
using RabbitMQ.Client;
using Xunit;
using Moq.Protected;
using System.Net;

namespace AcceptanceTest
{
    public class AcceptanceTest
    {
        private readonly Mock<IFeedbackRepository> _mockFeedbackRepository;
        private readonly Mock<IPaymentRepository> _mockPaymentRepository;
        private readonly Mock<IFeeRepository> _mockFeeRepository;
        private readonly Mock<HttpMessageHandler> _mockHttpHandler;
        private readonly HttpClient _mockHttpClient;
        private readonly IFeedbackService _feedbackService;
        private readonly IPaymentService _paymentService;
        private readonly IFeeService _feeService;

        public AcceptanceTest()
        {
            _mockFeedbackRepository = new Mock<IFeedbackRepository>();
            _mockPaymentRepository = new Mock<IPaymentRepository>();
            _mockFeeRepository = new Mock<IFeeRepository>();

            _mockHttpHandler = new Mock<HttpMessageHandler>();
            _mockHttpHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("1500")
                });

            _mockHttpClient = new HttpClient(_mockHttpHandler.Object);

            _feedbackService = new FeedbackService.Services.FeedbackService(_mockFeedbackRepository.Object);
            _paymentService = new PaymentService(_mockPaymentRepository.Object, _mockHttpClient);
            _feeService = new FeeService(_mockFeeRepository.Object, _mockHttpClient);
        }

        [Fact]
        public async Task Feedback_Payment_Fee()
        {
            var feedback = new Feedback
            {
                RestaurantID = 1,
                DeliveryDriverID = 1,
                FoodRating = 4.5,
                DeliveryRating = 4.8
            };

            var startDate = DateTime.UtcNow.AddDays(-30);
            var endDate = DateTime.UtcNow;

            _mockFeedbackRepository.Setup(repo => repo.AddFeedback(It.IsAny<Feedback>())).ReturnsAsync(feedback);
            _mockFeedbackRepository.Setup(repo => repo.GetFeedbackByDeliveryDriverId(1)).ReturnsAsync(new List<Feedback> { feedback });

            var createdFeedback = await _feedbackService.CreateFeedback(feedback);

            var payment = await _paymentService.CalculateAndSavePaymentAsync(1, startDate, endDate);

            var fee = await _feeService.CalculateAndSaveFeeAsync(1, startDate, endDate);

            createdFeedback.Should().NotBeNull();
            createdFeedback.FoodRating.Should().Be(4.5);
            createdFeedback.DeliveryRating.Should().Be(4.8);

            payment.Amount.Should().Be(42.00m);
            payment.Status.Should().Be("Pending");

            fee.Amount.Should().Be(61.00m);
            fee.Status.Should().Be("Pending");

            _mockPaymentRepository.Setup(repo => repo.UpdatePayment(It.IsAny<Payment>())).Returns(Task.CompletedTask);
            _mockPaymentRepository.Setup(repo => repo.GetPaymentById(It.IsAny<int>())).ReturnsAsync(payment);

            var paymentUpdated = await _paymentService.UpdatePaymentStatus(payment.PaymentID);

            paymentUpdated.Should().BeTrue();
            payment.Status.Should().Be("Paid");

            _mockFeeRepository.Setup(repo => repo.UpdateFee(It.IsAny<Fee>())).Returns(Task.CompletedTask);
            _mockFeeRepository.Setup(repo => repo.GetFeeById(It.IsAny<int>())).ReturnsAsync(fee);
            var feeUpdated = await _feeService.UpdateFeeStatus(fee.FeeID);

            feeUpdated.Should().BeTrue();
            fee.Status.Should().Be("Paid");
        }
    }
}