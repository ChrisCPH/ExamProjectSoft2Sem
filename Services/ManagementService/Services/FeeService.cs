using ManagementService.Models;
using ManagementService.Repositories;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace ManagementService.Services
{

    public interface IFeeService
    {
        Task<List<Fee>> GetAllFees();
        Task<Fee?> GetFeeById(int feeId);
        Task<Fee> CalculateAndSaveFeeAsync(int restaurantId, DateTime startDate, DateTime endDate);
        Task<decimal> GetTotalOrderPrice(int restaurantId, DateTime startDate, DateTime endDate);
        Task<int> GetOrderCount(int restaurantId, DateTime startDate, DateTime endDate);
        Task<bool> UpdateFeeStatus(int feeId);

    }

    public class FeeService : IFeeService
    {
        private readonly IFeeRepository _feeRepository;

        private readonly HttpClient _httpClient;

        private readonly string _rabbitMqHost = "localhost";  // RabbitMQ host

        private readonly string _queueName = "foodDeliveryAppQueue";  // Queue name

        public FeeService(IFeeRepository feeRepository, HttpClient httpClient)
        {
            _feeRepository = feeRepository;
            _httpClient = httpClient;
        }

        public async Task<List<Fee>> GetAllFees()
        {
            return await _feeRepository.GetAllFees();
        }

        public async Task<Fee?> GetFeeById(int feeId)
        {
            return await _feeRepository.GetFeeById(feeId);
        }

        public async Task<Fee> CalculateAndSaveFeeAsync(int restaurantId, DateTime startDate, DateTime endDate)
        {
            var totalOrderPrice = await GetTotalOrderPrice(restaurantId, startDate, endDate);
            var totalOrderCount = await GetOrderCount(restaurantId, startDate, endDate);

            var feeAmount = CalculateRestaurantFee(totalOrderPrice);

            var fee = new Fee
            {
                RestaurantID = restaurantId,
                Amount = feeAmount,
                TotalOrderPrice = totalOrderPrice,
                OrderCount = totalOrderCount,
                InvoiceDate = DateTime.Now,
                DueDate = DateTime.Now.AddDays(30),
                Status = "Pending"
            };

            await _feeRepository.AddFee(fee);

            PublishFeeMessage("RestaurantFee", fee);

            return fee;
        }

        public void PublishFeeMessage(string feeType, Fee fee)
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
                Type = feeType,
                fee.FeeID,
                fee.Amount,
                fee.RestaurantID,
                fee.OrderCount,
                fee.TotalOrderPrice,
                fee.InvoiceDate,
                fee.DueDate,
                fee.Status
            };

            var messageBody = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

            channel.BasicPublish(
                exchange: "",
                routingKey: _queueName,
                basicProperties: null,
                body: messageBody);

            Console.WriteLine($"Message published to queue: {JsonConvert.SerializeObject(message)}");
        }

        private decimal CalculateRestaurantFee(decimal totalOrderPrice)
        {
            decimal fee = 0;

            if (totalOrderPrice > 1000)
            {
                fee += (totalOrderPrice - 1000) * 0.03m;
                totalOrderPrice = 1000;
            }
            if (totalOrderPrice > 500)
            {
                fee += (totalOrderPrice - 500) * 0.04m;
                totalOrderPrice = 500;
            }
            if (totalOrderPrice > 100)
            {
                fee += (totalOrderPrice - 100) * 0.05m;
                totalOrderPrice = 100;
            }

            fee += totalOrderPrice * 0.06m;

            return Math.Round(fee, 2, MidpointRounding.AwayFromZero);
        }

        public async Task<decimal> GetTotalOrderPrice(int restaurantId, DateTime startDate, DateTime endDate)
        {
            var dateRange = new DateRequest
            {
                StartDate = startDate,
                EndDate = endDate
            };

            var content = new StringContent(System.Text.Json.JsonSerializer.Serialize(dateRange), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"http://localhost:5149/api/order/restaurant/{restaurantId}/totalprice", content);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return System.Text.Json.JsonSerializer.Deserialize<decimal>(json);
        }

        public async Task<int> GetOrderCount(int restaurantId, DateTime startDate, DateTime endDate)
        {
            var dateRange = new DateRequest
            {
                StartDate = startDate,
                EndDate = endDate
            };

            var content = new StringContent(System.Text.Json.JsonSerializer.Serialize(dateRange), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"http://localhost:5149/api/order/restaurant/{restaurantId}/ordercount", content);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return System.Text.Json.JsonSerializer.Deserialize<int>(json);
        }

        public async Task<bool> UpdateFeeStatus(int feeId)
        {
            var fee = await _feeRepository.GetFeeById(feeId);
            if (fee == null)
            {
                return false;
            }

            fee.PaidDate = DateTime.UtcNow;
            fee.Status = "Paid";

            await _feeRepository.UpdateFee(fee);

            return true;
        }
    }
}

