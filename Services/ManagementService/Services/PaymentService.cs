using ManagementService.Models;
using ManagementService.Repositories;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace ManagementService.Services
{
    public interface IPaymentService
    {
        Task<List<Payment>> GetAllPayments();
        Task<Payment?> GetPaymentById(int paymentId);
        Task<Payment> CalculateAndSavePaymentAsync(int driverId, DateTime startDate, DateTime endDate);
        Task<double> GetAvgRating(int driverId);
        Task<decimal> GetTotalDeliveryPrice(int driverId, DateTime startDate, DateTime endDate);
        Task<int> GetDeliveryCount(int driverId, DateTime startDate, DateTime endDate);
        Task<bool> UpdatePaymentStatus(int paymentId);
    }

    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepository;

        private readonly HttpClient _httpClient;

        private readonly string _rabbitMqHost = "localhost";  // RabbitMQ host

        private readonly string _queueName = "foodDeliveryAppQueue";  // Queue name

        public PaymentService(IPaymentRepository paymentRepository, HttpClient httpClient)
        {
            _paymentRepository = paymentRepository;
            _httpClient = httpClient;
        }

        public async Task<List<Payment>> GetAllPayments()
        {
            return await _paymentRepository.GetAllPayments();
        }

        public async Task<Payment?> GetPaymentById(int paymentId)
        {
            return await _paymentRepository.GetPaymentById(paymentId);
        }

        public async Task<Payment> CalculateAndSavePaymentAsync(int driverId, DateTime startDate, DateTime endDate)
        {
            var totalDeliveryPrice = await GetTotalDeliveryPrice(driverId, startDate, endDate);
            var totalDeliveryCount = await GetDeliveryCount(driverId, startDate, endDate);

            var avgRating = await GetAvgRating(driverId);

            var paymentAmount = CalculateDriverPayment(totalDeliveryPrice, avgRating);

            var payment = new Payment
            {
                DeliveryDriverID = driverId,
                Amount = paymentAmount,
                TotalDeliveryPrice = totalDeliveryPrice,
                DeliveryCount = totalDeliveryCount,
                PaycheckDate = DateTime.Now,
                Status = "Pending"
            };

            await _paymentRepository.AddPayment(payment);

            PublishPaymentMessage("DriverPayment", payment);

            return payment;
        }

        public void PublishPaymentMessage(string paymentType, Payment payment)
        {
            var factory = new ConnectionFactory()
            {
                HostName = _rabbitMqHost
            };

            using var connection = factory.CreateConnection();

            using var channel = connection.CreateModel();

            channel.QueueDeclare(queue: _queueName, durable: true, exclusive: false, autoDelete: false);

            var message = new
            {
                Type = paymentType,
                payment.PaymentID,
                payment.Amount,
                payment.DeliveryDriverID,
                payment.DeliveryCount,
                payment.TotalDeliveryPrice,
                payment.PaycheckDate,
                payment.Status
            };

            var messageBody = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

            channel.BasicPublish(
                exchange: "",
                routingKey: _queueName,
                basicProperties: null,
                body: messageBody);

            Console.WriteLine($"Message published to queue: {JsonConvert.SerializeObject(message)}");
        }

        private decimal CalculateDriverPayment(decimal totalDeliveryPrice, double avgRating)
        {
            decimal basePercentage;

            if (avgRating >= 4.5)
            {
                basePercentage = 0.028m;
            }
            else if (avgRating >= 4.0)
            {
                basePercentage = 0.026m;
            }
            else if (avgRating >= 3.5)
            {
                basePercentage = 0.024m;
            }
            else if (avgRating >= 3.0)
            {
                basePercentage = 0.022m;
            }
            else if (avgRating >= 2.5)
            {
                basePercentage = 0.020m;
            }
            else if (avgRating >= 2.0)
            {
                basePercentage = 0.018m;
            }
            else
            {
                basePercentage = 0.016m;
            }

            decimal payment = totalDeliveryPrice * basePercentage;
            return Math.Round(payment, 2, MidpointRounding.AwayFromZero);
        }

        public async Task<double> GetAvgRating(int driverId)
        {
            var response = await _httpClient.GetAsync($"http://localhost:5193/api/feedback/avgDeliveryRating/{driverId}");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return System.Text.Json.JsonSerializer.Deserialize<double>(json);
        }

        public async Task<decimal> GetTotalDeliveryPrice(int driverId, DateTime startDate, DateTime endDate)
        {
            var dateRange = new DateRequest
            {
                StartDate = startDate,
                EndDate = endDate
            };

            var content = new StringContent(System.Text.Json.JsonSerializer.Serialize(dateRange), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"http://localhost:5149/api/order/driver/{driverId}/totalprice", content);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return System.Text.Json.JsonSerializer.Deserialize<decimal>(json);
        }

        public async Task<int> GetDeliveryCount(int driverId, DateTime startDate, DateTime endDate)
        {
            var dateRange = new DateRequest
            {
                StartDate = startDate,
                EndDate = endDate
            };

            var content = new StringContent(System.Text.Json.JsonSerializer.Serialize(dateRange), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"http://localhost:5149/api/order/driver/{driverId}/ordercount", content);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return System.Text.Json.JsonSerializer.Deserialize<int>(json);
        }

        public async Task<bool> UpdatePaymentStatus(int paymentId)
        {
            var payment = await _paymentRepository.GetPaymentById(paymentId);
            if (payment == null)
            {
                return false;
            }

            payment.PaidDate = DateTime.UtcNow;
            payment.Status = "Paid";

            await _paymentRepository.UpdatePayment(payment);

            return true;
        }

    }
}