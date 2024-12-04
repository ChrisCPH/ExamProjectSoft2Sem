using System.Collections.Generic;
using System.Threading.Tasks;
using OrderService.Models;

namespace OrderService.Services
{
    public interface IOrderService
    {
        Task<int> CreateOrderAsync(int customerId, int restaurantId);
        Task CreateOrderItemAsync(int menuItemId, int orderId, int quantity);
        Task PlaceOrderAsync(int orderId);
        void PublishOrderMessage(string orderType, Order order, List<OrderItem> orderItems);
        Task<Order?> GetOrderByIdAsync(int orderId);
        Task<List<Order>> GetAllOrdersAsync();
        Task<List<OrderItem>> GetOrderItemsByOrderIdAsync(int orderId);
        Task<OrderItem?> GetOrderItemByIdAsync(int orderItemId);
        Task DeleteOrderAsync(int orderId);
        Task<Order> AddDriver(int orderId);
        Task<Order> OrderReadyForPickup(int orderId);
        Task<Order> AcceptOrder(int orderId);
        Task<Order> DeclineOrder(int orderId);
        Task<Order> DeliverOrder(int orderId);
        Task<bool> SetDriverUnavailable(int driverId);
    }

}