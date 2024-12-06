using OrderService.Models;
using OrderService.Repositories;
using System.Text.Json;
using System.Text.Json.Serialization;
using RabbitMQ.Client;
using Newtonsoft.Json;
using System.Text;

namespace OrderService.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly HttpClient _httpClient;
        private readonly string _rabbitMqHost = "localhost";  // RabbitMQ host
        private readonly string _queueName = "foodDeliveryAppQueue";  // Queue name

        public OrderService(IOrderRepository orderRepository, HttpClient httpClient)
        {
            _orderRepository = orderRepository;
            _httpClient = httpClient;
        }

        public async Task<int> CreateOrderAsync(int customerId, int restaurantId)
        {
            var order = new Order
            {
                CustomerID = customerId,
                RestaurantID = restaurantId,
                Status = "Pending",
                TotalPrice = 0
            };

            var orderId = await _orderRepository.AddOrderAsync(order);
            await _orderRepository.SaveChangesAsync();

            return orderId;
        }

        public async Task CreateOrderItemAsync(int menuItemId, int orderId, int quantity)
        {
            var price = await GetMenuItemPriceAsync(menuItemId);

            var orderItem = new OrderItem
            {
                OrderID = orderId,
                MenuItemID = menuItemId,
                Quantity = quantity,
                TotalPrice = price * quantity
            };

            await _orderRepository.AddOrderItemAsync(orderItem);
            await _orderRepository.SaveChangesAsync();
        }

        private async Task<decimal> GetMenuItemPriceAsync(int menuItemID)
        {
            var response = await _httpClient.GetAsync($"http://localhost:5045/api/restaurant/menu/{menuItemID}");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            var menuItem = System.Text.Json.JsonSerializer.Deserialize<MenuItem>(content);

            return menuItem?.Price ?? 0;
        }

        public async Task PlaceOrderAsync(int orderId)
        {
            var order = await _orderRepository.GetOrderByIdAsync(orderId);
            if (order == null)
            {
                throw new KeyNotFoundException("Order not found");
            }

            var orderItems = await _orderRepository.GetOrderItemsByOrderIdAsync(orderId);
            decimal totalOrderPrice = orderItems.Sum(item => item.TotalPrice);

            order.TotalPrice = totalOrderPrice;
            order.Status = "Placed";
            order.OrderPlaced = DateTime.UtcNow;

            PublishOrderMessage("OrderPlaced", order, orderItems);

            await _orderRepository.SaveChangesAsync();
        }

        public void PublishOrderMessage(string orderType, Order order, List<OrderItem> orderItems)
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
                Type = orderType,
                order.OrderID,
                order.CustomerID,
                order.RestaurantID,
                order.TotalPrice,
                order.Status,
                order.DriverID,
                OrderItems = orderItems.Select(item => new
                {
                    item.OrderItemID,
                    item.MenuItemID,
                    item.Quantity,
                    item.TotalPrice
                }).ToList()
            };

            var messageBody = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

            channel.BasicPublish(
                exchange: "",
                routingKey: _queueName,
                basicProperties: null,
                body: messageBody);

            Console.WriteLine($"Message published to queue: {JsonConvert.SerializeObject(message)}");
        }

        public async Task<Order?> GetOrderByIdAsync(int orderId)
        {
            return await _orderRepository.GetOrderByIdAsync(orderId);
        }

        public async Task<List<Order>> GetAllOrdersAsync()
        {
            return await _orderRepository.GetAllOrdersAsync();
        }

        public async Task<List<OrderItem>> GetOrderItemsByOrderIdAsync(int orderId)
        {
            return await _orderRepository.GetOrderItemsByOrderIdAsync(orderId);
        }

        public async Task<OrderItem?> GetOrderItemByIdAsync(int orderItemId)
        {
            return await _orderRepository.GetOrderItemByIdAsync(orderItemId);
        }

        public async Task DeleteOrderAsync(int orderId)
        {
            await _orderRepository.DeleteOrderAsync(orderId);
            await _orderRepository.SaveChangesAsync();
        }

        public async Task<Order> AddDriver(int orderId)
        {
            var order = await _orderRepository.GetOrderByIdAsync(orderId);
            if (order == null)
            {
                throw new KeyNotFoundException("Order not found");
            }

            var driverId = await GetAvailableDeliveryDriver();
            if (driverId == 0)
            {
                throw new Exception("No available driver.");
            }

            order.DriverID = driverId;

            return order;
        }

        private async Task<int> GetAvailableDeliveryDriver()
        {
            try
            {
                var response = await _httpClient.GetAsync($"http://localhost:5290/api/account/getDriverForDelivery");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var driverId = System.Text.Json.JsonSerializer.Deserialize<int>(content);

                    return driverId;
                }
                else
                {
                    Console.WriteLine($"Driver check failed: {response.ReasonPhrase}");
                    return 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calling API: {ex.Message}");
                return 0;
            }
        }

        public async Task<Order> OrderReadyForPickup(int orderId)
        {
            var order = await _orderRepository.GetOrderByIdAsync(orderId);
            if (order == null)
            {
                throw new KeyNotFoundException("Order not found");
            }

            order = await AddDriver(orderId);

            var orderItems = await _orderRepository.GetOrderItemsByOrderIdAsync(orderId);

            order.Status = "Ready for pickup";

            PublishOrderMessage("OrderReady", order, orderItems);

            await _orderRepository.SaveChangesAsync();
            return order;
        }

        public async Task<Order> AcceptOrder(int orderId)
        {
            var order = await _orderRepository.GetOrderByIdAsync(orderId);
            if (order == null)
            {
                throw new KeyNotFoundException("Order not found");
            }

            await SetDriverUnavailable(order.DriverID);

            order.Status = "Order accepted";

            await _orderRepository.SaveChangesAsync();
            return order;
        }

        public async Task<Order> DeclineOrder(int orderId)
        {
            var order = await _orderRepository.GetOrderByIdAsync(orderId);
            if (order == null)
            {
                throw new KeyNotFoundException("Order not found");
            }

            await SetDriverUnavailable(order.DriverID);

            order = await AddDriver(orderId);

            var orderItems = await _orderRepository.GetOrderItemsByOrderIdAsync(orderId);

            order.Status = "Ready for pickup";

            PublishOrderMessage("OrderReady", order, orderItems);

            await _orderRepository.SaveChangesAsync();
            return order;

        }

        public async Task<bool> SetDriverUnavailable(int driverId)
        {
            try
            {
                var url = $"http://localhost:5290/api/account/setUnavailable/{driverId}";

                var response = await _httpClient.PatchAsync(url, null);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Driver {driverId} marked as unavailable.");
                    return true;
                }
                else
                {
                    Console.WriteLine($"Failed to mark driver {driverId} as unavailable. Reason: {response.ReasonPhrase}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calling PATCH API: {ex.Message}");
                return false;
            }
        }

        public async Task<Order> DeliverOrder(int orderId)
        {
            var order = await _orderRepository.GetOrderByIdAsync(orderId);
            if (order == null)
            {
                throw new KeyNotFoundException("Order not found");
            }

            var orderItems = await _orderRepository.GetOrderItemsByOrderIdAsync(orderId);

            order.Status = "Delivered";
            order.OrderDelivered = DateTime.UtcNow;

            PublishOrderMessage("OrderDelivered", order, orderItems);

            await _orderRepository.SaveChangesAsync();
            return order;
        }

        public async Task<decimal> GetTotalOrderPriceRestaurant(int restaurantId, DateTime startDate, DateTime endDate)
        {

            return await _orderRepository.GetTotalOrderPriceRestaurant(restaurantId, startDate, endDate);
        }

        public async Task<int> GetOrderCountRestaurant(int restaurantId, DateTime startDate, DateTime endDate)
        {
            return await _orderRepository.GetOrderCountRestaurant(restaurantId, startDate, endDate);
        }

        public async Task<decimal> GetTotalOrderPriceDriver(int driverId, DateTime startDate, DateTime endDate)
        {

            return await _orderRepository.GetTotalOrderPriceDriver(driverId, startDate, endDate);
        }

        public async Task<int> GetOrderCountDriver(int driverId, DateTime startDate, DateTime endDate)
        {
            return await _orderRepository.GetOrderCountDriver(driverId, startDate, endDate);
        }
    }

    public class MenuItem
    {
        [JsonPropertyName("menuItemID")]
        public int MenuItemID { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("price")]
        public decimal Price { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("restaurantID")]
        public int RestaurantID { get; set; }
    }

}
