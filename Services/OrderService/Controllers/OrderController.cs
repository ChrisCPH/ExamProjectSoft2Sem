using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OrderService.Models;
using OrderService.Services;

namespace OrderService.Controllers
{
    [ApiController]
    [Route("api/order")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        // POST: /api/order?customerId=1&restaurantId=2
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromQuery] int customerId, [FromQuery] int restaurantId)
        {
            if (customerId <= 0 || restaurantId <= 0)
                return BadRequest("Invalid CustomerId or RestaurantId");

            var orderId = await _orderService.CreateOrderAsync(customerId, restaurantId);
            return CreatedAtAction(nameof(GetOrderById), new { orderId }, new { OrderId = orderId });
        }

        // POST: /api/order/1/items?menuItemId=456&quantity=3
        [HttpPost("{orderId}/items")]
        public async Task<IActionResult> CreateOrderItem(int orderId, [FromQuery] int menuItemId, [FromQuery] int quantity)
        {
            if (orderId <= 0 || menuItemId <= 0 || quantity <= 0)
                return BadRequest("Invalid orderId, menuItemId, or quantity");

            await _orderService.CreateOrderItemAsync(menuItemId, orderId, quantity);
            return Ok(new { Message = "Order item added successfully." });
        }

        // POST: /api/order/place/1
        [HttpPost("place/{orderId}")]
        public async Task<IActionResult> PlaceOrder(int orderId)
        {
            if (orderId <= 0)
                return BadRequest("Invalid orderId");

            try
            {
                await _orderService.PlaceOrderAsync(orderId);
                return Ok(new { Message = "Order placed successfully." });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { Message = "Order not found." });
            }
        }

        // GET: /api/order/1
        [HttpGet("{orderId}")]
        public async Task<IActionResult> GetOrderById(int orderId)
        {
            var order = await _orderService.GetOrderByIdAsync(orderId);
            if (order == null)
                return NotFound(new { Message = "No order with id: " + orderId });

            return Ok(order);
        }

        // GET: /api/order
        [HttpGet]
        public async Task<IActionResult> GetAllOrders()
        {
            var orders = await _orderService.GetAllOrdersAsync();
            if (orders == null)
                return NotFound(new { Message = "No orders found"});
            return Ok(orders);
        }

        // GET: /api/order/item/1
        [HttpGet("item/{orderItemId}")]
        public async Task<IActionResult> GetOrderItemById(int orderItemId)
        {
            var orderItem = await _orderService.GetOrderItemByIdAsync(orderItemId);
            if (orderItem == null)
                return NotFound();

            return Ok(orderItem);
        }

        // GET: /api/order/1/items
        [HttpGet("{orderId}/items")]
        public async Task<IActionResult> GetOrderItemsByOrderId(int orderId)
        {
            var orderItems = await _orderService.GetOrderItemsByOrderIdAsync(orderId);
            return Ok(orderItems);
        }

        // DELETE: /api/order/1
        [HttpDelete("{orderId}")]
        public async Task<IActionResult> DeleteOrder(int orderId)
        {
            await _orderService.DeleteOrderAsync(orderId);
            return NoContent();
        }
    }

}