using System.Threading.Tasks;
using Xunit;
using Moq;
using OrderService.Services;
using AccountService.Services;
using AccountService.Controllers;

namespace SystemTest
{
    public class SystemTest
    {
        private readonly Mock<IAccountService> _accountServiceMock;
        private readonly Mock<IOrderService> _orderServiceMock;

        public SystemTest()
        {
            _accountServiceMock = new Mock<IAccountService>();
            _orderServiceMock = new Mock<IOrderService>();
        }

        [Fact]
        public async Task OrderIsCreatedAndAssignedToDriver()
        {
            var customerId = 1;
            var restaurantId = 3;
            var menuItemId = 101;
            var quantity = 2;
            var driverId = 5;
            var orderId = 123;

            _accountServiceMock
                .Setup(service => service.GetAvailableDriverWithLongestWaitTime())
                .ReturnsAsync(driverId);

            _orderServiceMock
                .Setup(service => service.CreateOrderAsync(customerId, restaurantId))
                .ReturnsAsync(orderId);

            _orderServiceMock
                .Setup(service => service.CreateOrderItemAsync(menuItemId, orderId, quantity))
                .Returns(Task.CompletedTask);

            var createdOrderId = await _orderServiceMock.Object.CreateOrderAsync(customerId, restaurantId);
            var assignedDriverId = await _accountServiceMock.Object.GetAvailableDriverWithLongestWaitTime();
            await _orderServiceMock.Object.CreateOrderItemAsync(menuItemId, createdOrderId, quantity);

            Assert.Equal(orderId, createdOrderId);
            Assert.Equal(driverId, assignedDriverId);

            _orderServiceMock.Verify(service => service.CreateOrderAsync(customerId, restaurantId), Times.Once);
            _orderServiceMock.Verify(service => service.CreateOrderItemAsync(menuItemId, orderId, quantity), Times.Once);
            _accountServiceMock.Verify(service => service.GetAvailableDriverWithLongestWaitTime(), Times.Once);
        }

        
    }
}
