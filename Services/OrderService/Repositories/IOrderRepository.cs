using System.Collections.Generic;
using System.Threading.Tasks;
using OrderService.Models;

namespace OrderService.Repositories
{
    public interface IOrderRepository
    {
        Task<int> AddOrderAsync(Order order);
        Task AddOrderItemAsync(OrderItem orderItem);
        Task<Order?> GetOrderByIdAsync(int orderId);
        Task<List<Order>> GetAllOrdersAsync();
        Task<List<OrderItem>> GetOrderItemsByOrderIdAsync(int orderId);
        Task<OrderItem?> GetOrderItemByIdAsync(int orderItemId);
        Task DeleteOrderAsync(int orderId);
        Task<decimal> GetTotalOrderPriceRestaurant(int restaurantId, DateTime startDate, DateTime endDate);
        Task<int> GetOrderCountRestaurant(int restaurantId, DateTime startDate, DateTime endDate);
        Task<decimal> GetTotalOrderPriceDriver(int restaurantId, DateTime startDate, DateTime endDate);
        Task<int> GetOrderCountDriver(int driverId, DateTime startDate, DateTime endDate);
        Task SaveChangesAsync();
    }
}
