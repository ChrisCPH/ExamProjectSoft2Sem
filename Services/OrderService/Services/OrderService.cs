using System.Collections.Generic;
using System.Threading.Tasks;
using OrderService.Models;
using OrderService.Repositories;
using System.Text.Json;
using System.Text.Json.Serialization;


namespace OrderService.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly HttpClient _httpClient;

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

            Console.WriteLine(content); 

            var menuItem = JsonSerializer.Deserialize<MenuItem>(content);

            Console.WriteLine(menuItem.Price);

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

            // TODO: add notification stuff
            //await NotifyRestaurantAsync(order.RestaurantID, orderId);
            await _orderRepository.SaveChangesAsync();
        }

        private async Task NotifyRestaurantAsync(int restaurantId, int orderId)
        {
            var notification = new
            {
                RestaurantId = restaurantId,
                OrderId = orderId,
                Message = "You have a new order."
            };

            var content = new StringContent(JsonSerializer.Serialize(notification), System.Text.Encoding.UTF8, "application/json");
            await _httpClient.PostAsync("/restaurant/notifications", content);
        }

        public async Task<Order> GetOrderByIdAsync(int orderId)
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

        public async Task<OrderItem> GetOrderItemByIdAsync(int orderItemId)
        {
            return await _orderRepository.GetOrderItemByIdAsync(orderItemId);
        }

        public async Task DeleteOrderAsync(int orderId)
        {
            await _orderRepository.DeleteOrderAsync(orderId);
            await _orderRepository.SaveChangesAsync();
        }
    }

    public class MenuItem
    {
        [JsonPropertyName("menuItemID")]
        public int MenuItemID { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("price")]
        public decimal Price { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("restaurantID")]
        public int RestaurantID { get; set; }
    }

}
