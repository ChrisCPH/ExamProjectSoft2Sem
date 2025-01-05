using Xunit;
using Moq;
using SearchService.Controllers;
using SearchService.Repositories;
using SearchService.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace SearchService.Tests
{
    public class RestaurantControllerTests
    {
        private readonly Mock<IRestaurantRepository> _mockRepository;
        private readonly RestaurantController _controller;

        public RestaurantControllerTests()
        {
            _mockRepository = new Mock<IRestaurantRepository>();
            _controller = new RestaurantController(_mockRepository.Object);
        }

        [Fact]
        public async Task SearchRestaurant_ReturnsOk_WithResults()
        {
            var restaurants = new List<Restaurant> { new Restaurant { RestaurantID = 1, Name = "Test Restaurant" } };
            _mockRepository.Setup(repo => repo.SearchRestaurantAsync("Test", null, null))
                .ReturnsAsync(restaurants);

            var result = await _controller.SearchRestaurant("Test", null, null);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(restaurants, okResult.Value);
        }

        [Fact]
        public async Task GetRestaurantById_ReturnsOk_WhenRestaurantExists()
        {
            var restaurant = new Restaurant { RestaurantID = 1, Name = "Test Restaurant" };
            _mockRepository.Setup(repo => repo.GetRestaurantByIdAsync(1)).ReturnsAsync(restaurant);

            var result = await _controller.GetRestaurantById(1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(restaurant, okResult.Value);
        }

        [Fact]
        public async Task GetRestaurantById_ReturnsNotFound_WhenRestaurantDoesNotExist()
        {
            _mockRepository.Setup(repo => repo.GetRestaurantByIdAsync(1)).ReturnsAsync((Restaurant)null!);

            var result = await _controller.GetRestaurantById(1);

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task AddRestaurantAsync_ReturnsOk_WithCreatedRestaurant()
        {
            var newRestaurant = new Restaurant { Name = "New Restaurant" };
            var createdRestaurant = new Restaurant { RestaurantID = 1, Name = "New Restaurant" };
            _mockRepository.Setup(repo => repo.AddRestaurantAsync(newRestaurant)).ReturnsAsync(createdRestaurant);

            var result = await _controller.AddRestaurantAsync(newRestaurant);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(createdRestaurant, okResult.Value);
        }

        [Fact]
        public async Task DeleteRestaurantAsync_ReturnsNotFound_WhenRestaurantDoesNotExist()
        {
            _mockRepository.Setup(repo => repo.DeleteRestaurantAsync(1)).ReturnsAsync((string)null!);

            var result = await _controller.DeleteRestaurantAsync(1);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task UpdateRestaurantAsync_ReturnsOk_WhenUpdateSucceeds()
        {
            var updatedRestaurant = new Restaurant { RestaurantID = 1, Name = "Updated Restaurant" };
            _mockRepository.Setup(repo => repo.UpdateRestaurantAsync(updatedRestaurant)).ReturnsAsync(updatedRestaurant);

            var result = await _controller.UpdateRestaurantAsync(updatedRestaurant);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(updatedRestaurant, okResult.Value);
        }

        [Fact]
        public async Task UpdateRestaurantAsync_ReturnsNotFound_WhenRestaurantDoesNotExist()
        {
            var updatedRestaurant = new Restaurant { RestaurantID = 1, Name = "Updated Restaurant" };
            _mockRepository.Setup(repo => repo.UpdateRestaurantAsync(updatedRestaurant)).ReturnsAsync((Restaurant)null!);

            var result = await _controller.UpdateRestaurantAsync(updatedRestaurant);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetMenu_ReturnsOk_WhenMenuExists()
        {
            var menu = new List<MenuItem> { new MenuItem { RestaurantID = 1, Name = "Pizza" } };
            _mockRepository.Setup(repo => repo.GetMenuAsync(1)).ReturnsAsync(menu);

            var result = await _controller.GetMenu(1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(menu, okResult.Value);
        }

        [Fact]
        public async Task GetMenu_ReturnsNotFound_WhenMenuDoesNotExist()
        {
            _mockRepository.Setup(repo => repo.GetMenuAsync(1)).ReturnsAsync((List<MenuItem>)null!);

            var result = await _controller.GetMenu(1);

            Assert.IsType<NotFoundResult>(result);
        }
    }
}
