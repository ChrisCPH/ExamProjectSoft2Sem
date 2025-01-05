using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using OrderService.Controllers;
using OrderService.Models;
using OrderService.Services;
using Xunit;

public class OrderControllerTests
{
    private readonly Mock<IOrderService> _orderServiceMock;
    private readonly OrderController _controller;

    public OrderControllerTests()
    {
        _orderServiceMock = new Mock<IOrderService>();
        _controller = new OrderController(_orderServiceMock.Object);
    }

    [Fact]
    public async Task CreateOrder_ValidInput_ReturnsCreated()
    {
        _orderServiceMock.Setup(s => s.CreateOrderAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(1);

        var result = await _controller.CreateOrder(1, 2);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(1, createdResult.RouteValues!["orderId"]);
    }

    [Fact]
    public async Task CreateOrder_InvalidInput_ReturnsBadRequest()
    {
        var result = await _controller.CreateOrder(0, -1);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task GetOrderById_ExistingOrder_ReturnsOk()
    {
        var order = new Order();
        _orderServiceMock.Setup(s => s.GetOrderByIdAsync(It.IsAny<int>())).ReturnsAsync(order);

        var result = await _controller.GetOrderById(1);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(order, okResult.Value);
    }

    [Fact]
    public async Task GetOrderById_NonExistingOrder_ReturnsNotFound()
    {
        _orderServiceMock.Setup(s => s.GetOrderByIdAsync(It.IsAny<int>())).ReturnsAsync((Order)null!);

        var result = await _controller.GetOrderById(1);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task PlaceOrder_ValidOrder_ReturnsOk()
    {
        _orderServiceMock.Setup(s => s.PlaceOrderAsync(It.IsAny<int>())).Returns(Task.CompletedTask);

        var result = await _controller.PlaceOrder(1);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task PlaceOrder_NonExistingOrder_ReturnsNotFound()
    {
        _orderServiceMock.Setup(s => s.PlaceOrderAsync(It.IsAny<int>())).Throws<KeyNotFoundException>();

        var result = await _controller.PlaceOrder(1);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task GetAllOrders_ReturnsOk()
    {
        var orders = new List<Order>();
        _orderServiceMock.Setup(s => s.GetAllOrdersAsync()).ReturnsAsync(orders);

        var result = await _controller.GetAllOrders();

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(orders, okResult.Value);
    }

    [Fact]
    public async Task DeleteOrder_ValidOrder_ReturnsNoContent()
    {
        _orderServiceMock.Setup(s => s.DeleteOrderAsync(It.IsAny<int>())).Returns(Task.CompletedTask);

        var result = await _controller.DeleteOrder(1);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task ReadyForPickup_ValidOrder_ReturnsOk()
    {
        var order = new Order();
        _orderServiceMock.Setup(s => s.OrderReadyForPickup(It.IsAny<int>())).ReturnsAsync(order);

        var result = await _controller.ReadyForPickup(1);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(order, okResult.Value);
    }

    [Fact]
    public async Task AcceptOrder_ValidOrder_ReturnsOk()
    {
        var order = new Order();
        _orderServiceMock.Setup(s => s.AcceptOrder(It.IsAny<int>())).ReturnsAsync(order);

        var result = await _controller.AcceptOrder(1);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(order, okResult.Value);
    }

    [Fact]
    public async Task DeclineOrder_ValidOrder_ReturnsOk()
    {
        var order = new Order();
        _orderServiceMock.Setup(s => s.DeclineOrder(It.IsAny<int>())).ReturnsAsync(order);

        var result = await _controller.DeclineOrder(1);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(order, okResult.Value);
    }

    [Fact]
    public async Task DeliverOrder_ValidOrder_ReturnsOk()
    {
        var order = new Order();
        _orderServiceMock.Setup(s => s.DeliverOrder(It.IsAny<int>())).ReturnsAsync(order);

        var result = await _controller.DeliverOrder(1);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(order, okResult.Value);
    }
}
