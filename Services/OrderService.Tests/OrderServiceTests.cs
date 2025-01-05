using Moq;
using System.Net;
using System.Text.Json;
using OrderService.Services;
using OrderService.Repositories;
using OrderService.Models;
using OrderService.Data;
using Microsoft.EntityFrameworkCore;
using Moq.Protected;

public class OrderServiceTests
{

    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly OrderService.Services.OrderService _orderService;
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly HttpClient _httpClient;


    public OrderServiceTests()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        _httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _orderService = new OrderService.Services.OrderService(_orderRepositoryMock.Object, _httpClient);
    }

    private OrderDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<OrderDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new OrderDbContext(options);
    }

    private async Task SeedData(OrderDbContext context)
    {
        var orders = new List<Order>
        {
            new Order
            {
                OrderID = 1,
                CustomerID = 101,
                RestaurantID = 201,
                Status = "Pending",
                TotalPrice = 0,
                DriverID = 2,
                OrderPlaced = new DateTime(2025, 01, 05),
                OrderDelivered = new DateTime(2000, 01, 01) 
            },
            new Order
            {
                OrderID = 2,
                CustomerID = 102,
                RestaurantID = 202,
                Status = "Delivered",
                TotalPrice = 50.00m,
                DriverID = 1,
                OrderPlaced = new DateTime(2025, 01, 05),
                OrderDelivered = new DateTime(2025, 01, 25)
            }
        };

        var orderItems = new List<OrderItem>
        {
            new OrderItem { OrderItemID = 1, OrderID = 1, MenuItemID = 301, Quantity = 2, TotalPrice = 25.00m },
            new OrderItem { OrderItemID = 2, OrderID = 1, MenuItemID = 302, Quantity = 1, TotalPrice = 15.00m },
            new OrderItem { OrderItemID = 3, OrderID = 2, MenuItemID = 303, Quantity = 5, TotalPrice = 50.00m }
        };

        context.Order.AddRange(orders);
        context.OrderItem.AddRange(orderItems);
        await context.SaveChangesAsync();
    }

    [Fact]
    public async Task AddOrderAsync_AddsOrderToDatabase()
    {
        var context = GetInMemoryDbContext();
        await SeedData(context);
        var repository = new OrderRepository(context);

        var newOrder = new Order
        {
            OrderID = 3,
            CustomerID = 103,
            RestaurantID = 203,
            Status = "Pending",
            TotalPrice = 75.00m
        };

        var orderId = await repository.AddOrderAsync(newOrder);

        var orders = context.Order.ToList();
        Assert.Equal(3, orders.Count);
        Assert.Contains(orders, o => o.OrderID == orderId && o.CustomerID == 103 && o.TotalPrice == 75.00m);
    }

    [Fact]
    public async Task GetOrderByIdAsync_ReturnsCorrectOrder()
    {
        var context = GetInMemoryDbContext();
        await SeedData(context);
        var repository = new OrderRepository(context);

        var order = await repository.GetOrderByIdAsync(1);

        Assert.NotNull(order);
        Assert.Equal(101, order.CustomerID);
        Assert.Equal("Pending", order.Status);
    }

    [Fact]
    public async Task GetAllOrdersAsync_ReturnsAllOrders()
    {
        var context = GetInMemoryDbContext();
        await SeedData(context);
        var repository = new OrderRepository(context);

        var orders = await repository.GetAllOrdersAsync();

        Assert.NotNull(orders);
        Assert.Equal(2, orders.Count);
        Assert.Contains(orders, o => o.OrderID == 1);
        Assert.Contains(orders, o => o.OrderID == 2);
    }

    [Fact]
    public async Task CreateOrderItemAsync_AddsOrderItemToDatabase()
    {
        var context = GetInMemoryDbContext();
        await SeedData(context);
        var repository = new OrderRepository(context);

        var newOrderItem = new OrderItem
        {
            OrderItemID = 4,
            OrderID = 1,
            MenuItemID = 304,
            Quantity = 3,
            TotalPrice = 30.00m
        };

        await repository.AddOrderItemAsync(newOrderItem);
        await repository.SaveChangesAsync();

        var orderItems = context.OrderItem.ToList();
        Assert.Equal(4, orderItems.Count);
        Assert.Contains(orderItems, oi => oi.OrderItemID == 4 && oi.TotalPrice == 30.00m);
    }

    [Fact]
    public async Task GetOrderItemByIdAsync_ReturnsCorrectOrderItem()
    {
        var context = GetInMemoryDbContext();
        await SeedData(context);
        var repository = new OrderRepository(context);

        var orderItem = await repository.GetOrderItemByIdAsync(1);

        Assert.NotNull(orderItem);
        Assert.Equal(301, orderItem.MenuItemID);
        Assert.Equal(25.00m, orderItem.TotalPrice);
    }

    [Fact]
    public async Task GetOrderItemsByOrderIdAsync_ReturnsAllItemsForOrder()
    {
        var context = GetInMemoryDbContext();
        await SeedData(context);
        var repository = new OrderRepository(context);

        var orderItems = await repository.GetOrderItemsByOrderIdAsync(1);

        Assert.NotNull(orderItems);
        Assert.Equal(2, orderItems.Count);
        Assert.All(orderItems, oi => Assert.Equal(1, oi.OrderID));
    }

    [Fact]
    public async Task DeleteOrderAsync_RemovesOrderAndAssociatedItems()
    {
        var context = GetInMemoryDbContext();
        await SeedData(context);
        var repository = new OrderRepository(context);

        await repository.DeleteOrderAsync(1);
        await repository.SaveChangesAsync();

        var orders = context.Order.ToList();
        var orderItems = context.OrderItem.ToList();

        Assert.Single(orders);
        Assert.DoesNotContain(orders, o => o.OrderID == 1);
        Assert.All(orderItems, oi => Assert.NotEqual(1, oi.OrderID));
    }

    [Fact]
    public async Task DeleteOrderAsync_DoesNothingIfOrderNotFound()
    {
        var context = GetInMemoryDbContext();
        await SeedData(context);
        var repository = new OrderRepository(context);

        await repository.DeleteOrderAsync(999);
        await repository.SaveChangesAsync();

        var orders = context.Order.ToList();
        var orderItems = context.OrderItem.ToList();

        Assert.Equal(2, orders.Count);
        Assert.Equal(3, orderItems.Count);
    }

    [Fact]
    public async Task CreateOrderAsync_ShouldAddOrderAndReturnId()
    {
        var mockRepository = new Mock<IOrderRepository>();
        var service = new OrderService.Services.OrderService(mockRepository.Object, new HttpClient());
        var customerId = 1;
        var restaurantId = 2;
        var expectedOrderId = 42;

        mockRepository.Setup(r => r.AddOrderAsync(It.IsAny<Order>())).ReturnsAsync(expectedOrderId);

        var result = await service.CreateOrderAsync(customerId, restaurantId);

        Assert.Equal(expectedOrderId, result);
        mockRepository.Verify(r => r.AddOrderAsync(It.Is<Order>(o =>
            o.CustomerID == customerId &&
            o.RestaurantID == restaurantId &&
            o.Status == "Pending" &&
            o.TotalPrice == 0)), Times.Once);
        mockRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    public class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _sendFunc;

        public MockHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> sendFunc)
        {
            _sendFunc = sendFunc;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(_sendFunc(request));
        }
    }

    [Fact]
    public async Task CreateOrderItemAsync_ShouldAddOrderItemWithCorrectTotalPrice()
    {
        var menuItemId = 1;
        var orderId = 10;
        var quantity = 3;
        var menuItemPrice = 15.50m;

        var menuItemResponse = JsonSerializer.Serialize(new MenuItem { Price = menuItemPrice });
        var handler = new MockHttpMessageHandler(request =>
        {
            Assert.Equal($"http://localhost:5045/api/restaurant/menu/{menuItemId}", request.RequestUri?.ToString());
            Assert.Equal(HttpMethod.Get, request.Method);

            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(menuItemResponse, System.Text.Encoding.UTF8, "application/json")
            };
        });

        var httpClient = new HttpClient(handler);

        var mockRepository = new Mock<IOrderRepository>();
        var service = new OrderService.Services.OrderService(mockRepository.Object, httpClient);

        await service.CreateOrderItemAsync(menuItemId, orderId, quantity);

        mockRepository.Verify(repo => repo.AddOrderItemAsync(It.Is<OrderItem>(oi =>
            oi.OrderID == orderId &&
            oi.MenuItemID == menuItemId &&
            oi.Quantity == quantity &&
            oi.TotalPrice == menuItemPrice * quantity
        )), Times.Once);

        mockRepository.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task PlaceOrderAsync_ShouldUpdateOrderStatusAndTotalPrice()
    {
        var mockRepository = new Mock<IOrderRepository>();
        var service = new OrderService.Services.OrderService(mockRepository.Object, new HttpClient());
        var orderId = 1;
        var restaurantId = 2;
        var order = new Order { OrderID = orderId, RestaurantID = restaurantId, Status = "Pending", TotalPrice = 0 };
        var orderItems = new List<OrderItem>
        {
            new OrderItem { TotalPrice = 10 },
            new OrderItem { TotalPrice = 20 }
        };

        mockRepository.Setup(r => r.GetOrderByIdAsync(orderId)).ReturnsAsync(order);
        mockRepository.Setup(r => r.GetOrderItemsByOrderIdAsync(orderId)).ReturnsAsync(orderItems);

        await service.PlaceOrderAsync(orderId);

        Assert.Equal("Placed", order.Status);
        Assert.Equal(30, order.TotalPrice);

        mockRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeliverOrder_ShouldUpdateOrderStatus_WhenOrderExists()
    {
        var dbContext = GetInMemoryDbContext();
        await SeedData(dbContext);

        var repository = new OrderRepository(dbContext);
        var orderService = new OrderService.Services.OrderService(repository, new HttpClient());

        var result = await orderService.DeliverOrder(1);

        Assert.NotNull(result);
        Assert.Equal("Delivered", result.Status);

        var updatedOrder = await dbContext.Order.FindAsync(1);
        Assert.Equal("Delivered", updatedOrder?.Status);
    }

    [Fact]
    public async Task DeliverOrder_ShouldThrowException_WhenOrderDoesNotExist()
    {
        var dbContext = GetInMemoryDbContext();
        await SeedData(dbContext);

        var repository = new OrderRepository(dbContext);
        var orderService = new OrderService.Services.OrderService(repository, new HttpClient());

        await Assert.ThrowsAsync<KeyNotFoundException>(() => orderService.DeliverOrder(999));
    }

    [Fact]
    public async Task AcceptOrder_ShouldUpdateStatus_WhenOrderExists()
    {
        var dbContext = GetInMemoryDbContext();
        await SeedData(dbContext);
        var repository = new OrderRepository(dbContext);
        var orderService = new OrderService.Services.OrderService(repository, new HttpClient());

        var result = await orderService.AcceptOrder(1);

        Assert.NotNull(result);
        Assert.Equal("Order accepted", result.Status);

        var updatedOrder = await dbContext.Order.FindAsync(1);
        Assert.Equal("Order accepted", updatedOrder?.Status);
    }

    [Fact]
    public async Task AcceptOrder_ShouldThrowException_WhenOrderDoesNotExist()
    {
        var dbContext = GetInMemoryDbContext();
        await SeedData(dbContext);
        var repository = new OrderRepository(dbContext);
        var orderService = new OrderService.Services.OrderService(repository, new HttpClient());

        await Assert.ThrowsAsync<KeyNotFoundException>(() => orderService.AcceptOrder(999));
    }

    [Fact]
    public async Task GetOrderByIdAsync_ExistingOrder_ReturnsOrder()
    {
        var order = new Order();
        _orderRepositoryMock.Setup(r => r.GetOrderByIdAsync(It.IsAny<int>())).ReturnsAsync(order);

        var result = await _orderService.GetOrderByIdAsync(1);

        Assert.Equal(order, result);
    }

    [Fact]
    public async Task GetOrderByIdAsync_NonExistingOrder_ReturnsNull()
    {
        _orderRepositoryMock.Setup(r => r.GetOrderByIdAsync(It.IsAny<int>())).ReturnsAsync((Order)null!);

        var result = await _orderService.GetOrderByIdAsync(1);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllOrdersAsync_ReturnsOrders()
    {
        var orders = new List<Order> { new Order(), new Order() };
        _orderRepositoryMock.Setup(r => r.GetAllOrdersAsync()).ReturnsAsync(orders);

        var result = await _orderService.GetAllOrdersAsync();

        Assert.Equal(orders, result);
    }

    [Fact]
    public async Task GetOrderItemsByOrderIdAsync_ValidOrderId_ReturnsOrderItems()
    {
        var orderItems = new List<OrderItem> { new OrderItem(), new OrderItem() };
        _orderRepositoryMock.Setup(r => r.GetOrderItemsByOrderIdAsync(It.IsAny<int>())).ReturnsAsync(orderItems);

        var result = await _orderService.GetOrderItemsByOrderIdAsync(1);

        Assert.Equal(orderItems, result);
    }

    [Fact]
    public async Task GetOrderItemsByOrderIdAsync_InvalidOrderId_ReturnsEmptyList()
    {
        _orderRepositoryMock.Setup(r => r.GetOrderItemsByOrderIdAsync(It.IsAny<int>())).ReturnsAsync(new List<OrderItem>());

        var result = await _orderService.GetOrderItemsByOrderIdAsync(1);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetOrderItemByIdAsync_ExistingOrderItem_ReturnsOrderItem()
    {
        var orderItem = new OrderItem();
        _orderRepositoryMock.Setup(r => r.GetOrderItemByIdAsync(It.IsAny<int>())).ReturnsAsync(orderItem);

        var result = await _orderService.GetOrderItemByIdAsync(1);

        Assert.Equal(orderItem, result);
    }

    [Fact]
    public async Task GetOrderItemByIdAsync_NonExistingOrderItem_ReturnsNull()
    {
        _orderRepositoryMock.Setup(r => r.GetOrderItemByIdAsync(It.IsAny<int>())).ReturnsAsync((OrderItem)null!);

        var result = await _orderService.GetOrderItemByIdAsync(1);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetTotalOrderPriceRestaurant_ValidInput_ReturnsTotalPrice()
    {
        var expectedPrice = 100.50m;
        _orderRepositoryMock.Setup(r => r.GetTotalOrderPriceRestaurant(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(expectedPrice);

        var result = await _orderService.GetTotalOrderPriceRestaurant(1, DateTime.Now.AddDays(-7), DateTime.Now);

        Assert.Equal(expectedPrice, result);
    }

    [Fact]
    public async Task GetOrderCountRestaurant_ValidInput_ReturnsOrderCount()
    {
        var expectedCount = 25;
        _orderRepositoryMock.Setup(r => r.GetOrderCountRestaurant(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(expectedCount);

        var result = await _orderService.GetOrderCountRestaurant(1, DateTime.Now.AddDays(-7), DateTime.Now);

        Assert.Equal(expectedCount, result);
    }

    [Fact]
    public async Task GetTotalOrderPriceDriver_ValidInput_ReturnsTotalPrice()
    {
        var expectedPrice = 200.75m;
        _orderRepositoryMock.Setup(r => r.GetTotalOrderPriceDriver(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(expectedPrice);

        var result = await _orderService.GetTotalOrderPriceDriver(1, DateTime.Now.AddDays(-7), DateTime.Now);

        Assert.Equal(expectedPrice, result);
    }

    [Fact]
    public async Task GetOrderCountDriver_ValidInput_ReturnsOrderCount()
    {
        var expectedCount = 30;
        _orderRepositoryMock.Setup(r => r.GetOrderCountDriver(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(expectedCount);

        var result = await _orderService.GetOrderCountDriver(1, DateTime.Now.AddDays(-7), DateTime.Now);

        Assert.Equal(expectedCount, result);
    }

    [Fact]
    public async Task SetDriverUnavailable_SuccessfulResponse_ReturnsTrue()
    {
        var driverId = 1;
        var url = $"http://localhost:5290/api/account/setUnavailable/{driverId}";
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Patch && req.RequestUri!.ToString() == url),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK
            });

        var result = await _orderService.SetDriverUnavailable(driverId);

        Assert.True(result);
    }

    [Fact]
    public async Task SetDriverUnavailable_UnsuccessfulResponse_ReturnsFalse()
    {
        var driverId = 1;
        var url = $"http://localhost:5290/api/account/setUnavailable/{driverId}";
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Patch && req.RequestUri!.ToString() == url),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest,
                ReasonPhrase = "Bad Request"
            });

        var result = await _orderService.SetDriverUnavailable(driverId);

        Assert.False(result);
    }

    [Fact]
    public async Task SetDriverUnavailable_ExceptionThrown_ReturnsFalse()
    {
        var driverId = 1;
        var url = $"http://localhost:5290/api/account/setUnavailable/{driverId}";
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Patch && req.RequestUri!.ToString() == url),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Network error"));

        var result = await _orderService.SetDriverUnavailable(driverId);

        Assert.False(result);
    }

    [Fact]
    public async Task GetTotalOrderPriceRestaurant()
    {
        var context = GetInMemoryDbContext();
        await SeedData(context);
        var orderRepository = new OrderRepository(context);

        int restaurantId = 202;
        DateTime startDate = new DateTime(2025, 01, 01);
        DateTime endDate = new DateTime(2025, 01, 30);

        var totalPrice = await orderRepository.GetTotalOrderPriceRestaurant(restaurantId, startDate, endDate);

        Assert.Equal(50.00m, totalPrice);
    }

    [Fact]
    public async Task GetOrderCountRestaurant()
    {
        var context = GetInMemoryDbContext();
        await SeedData(context);
        var orderRepository = new OrderRepository(context);

        int restaurantId = 202;
        DateTime startDate = new DateTime(2025, 01, 01);
        DateTime endDate = new DateTime(2025, 01, 30);

        var orderCount = await orderRepository.GetOrderCountRestaurant(restaurantId, startDate, endDate);

        Assert.Equal(1, orderCount);
    }

    [Fact]
    public async Task GetTotalOrderPriceDriver()
    {
        var context = GetInMemoryDbContext();
        await SeedData(context);
        var orderRepository = new OrderRepository(context);

        int driverId = 1;
        DateTime startDate = new DateTime(2025, 01, 01);
        DateTime endDate = new DateTime(2025, 01, 30);

        var totalPrice = await orderRepository.GetTotalOrderPriceDriver(driverId, startDate, endDate);

        Assert.Equal(50.00m, totalPrice);
    }

    [Fact]
    public async Task GetOrderCountDriver()
    {
        var context = GetInMemoryDbContext();
        await SeedData(context);
        var orderRepository = new OrderRepository(context);

        int driverId = 1;
        DateTime startDate = new DateTime(2025, 01, 01);
        DateTime endDate = new DateTime(2025, 01, 30);

        var orderCount = await orderRepository.GetOrderCountDriver(driverId, startDate, endDate);

        Assert.Equal(1, orderCount);
    }
}