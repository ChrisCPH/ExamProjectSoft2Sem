using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OrderService.Models;
using OrderService.Data;

namespace OrderService.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly OrderDbContext _context;

        public OrderRepository(OrderDbContext context)
        {
            _context = context;
        }

        public async Task<int> AddOrderAsync(Order order)
        {
            _context.Order.Add(order);
            await _context.SaveChangesAsync();
            return order.OrderID;
        }

        public async Task AddOrderItemAsync(OrderItem orderItem)
        {
            _context.OrderItem.Add(orderItem);
            await _context.SaveChangesAsync();
        }

        public async Task<Order> GetOrderByIdAsync(int orderId)
        {
            return await _context.Order
                .FirstOrDefaultAsync(o => o.OrderID == orderId);
        }

        public async Task<List<Order>> GetAllOrdersAsync()
        {
            return await _context.Order.ToListAsync();
        }

        public async Task<List<OrderItem>> GetOrderItemsByOrderIdAsync(int orderId)
        {
            return await _context.OrderItem
                .Where(oi => oi.OrderID == orderId)
                .ToListAsync();
        }

        public async Task<OrderItem> GetOrderItemByIdAsync(int orderItemId)
        {
            return await _context.OrderItem
                .FirstOrDefaultAsync(oi => oi.OrderItemID == orderItemId);
        }

        public async Task DeleteOrderAsync(int orderId)
        {
            var order = await _context.Order
                .FirstOrDefaultAsync(o => o.OrderID == orderId);

            if (order != null)
            {
                var orderItem = await _context.OrderItem
                    .Where(oi => oi.OrderID == orderId)
                    .ToListAsync();

                _context.OrderItem.RemoveRange(orderItem);
                await _context.SaveChangesAsync();
                _context.Order.Remove(order);
                await _context.SaveChangesAsync();
            }
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
