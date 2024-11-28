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
        Task<Order?> GetOrderByIdAsync(int orderId);
        Task<List<Order>> GetAllOrdersAsync();
        Task<List<OrderItem>> GetOrderItemsByOrderIdAsync(int orderId);
        Task<OrderItem?> GetOrderItemByIdAsync(int orderItemId);
        Task DeleteOrderAsync(int orderId);
        Task<Order> AddDriver(int orderId, int driverID);
    }

}