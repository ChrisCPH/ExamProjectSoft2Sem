using Microsoft.EntityFrameworkCore;
using SearchService.Data;
using SearchService.Models;
using SearchService.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace SearchService.Tests
{
    public class RestaurantRepositoryTests
    {
        private SearchDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<SearchDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new SearchDbContext(options);
        }

        private async Task SeedData(SearchDbContext context)
        {
            var restaurant = new List<Restaurant>
            {
                new Restaurant
                {
                    RestaurantID = 1,
                    Name = "Italian Bistro",
                    Address = "123 Main St",
                    Category = "Italian",
                    Menu = new List<MenuItem>
                    {
                        new MenuItem { MenuItemID = 1, Name = "Pasta", Price = 12.99m },
                        new MenuItem { MenuItemID = 2, Name = "Pizza", Price = 15.49m }
                    }
                },
                new Restaurant
                {
                    RestaurantID = 2,
                    Name = "Sushi World",
                    Address = "456 Elm St",
                    Category = "Japanese",
                    Menu = new List<MenuItem>
                    {
                        new MenuItem { MenuItemID = 3, Name = "Sushi Roll", Price = 8.99m },
                        new MenuItem { MenuItemID = 4, Name = "Tempura", Price = 10.99m }
                    }
                },
                new Restaurant
                {
                    RestaurantID = 3,
                    Name = "Burger Haven",
                    Address = "789 Oak St",
                    Category = "American",
                    Menu = null
                }
            };

            context.Restaurant.AddRange(restaurant);
            await context.SaveChangesAsync();
        }

        [Fact]
        public async Task SearchRestaurantAsync_ReturnsRestaurantByName()
        {
            var context = GetInMemoryDbContext();
            await SeedData(context);
            var repository = new RestaurantRepository(context);

            var results = await repository.SearchRestaurantAsync(name: "Sushi");

            Assert.Single(results);
            Assert.Equal("Sushi World", results[0].Name);
        }

        [Fact]
        public async Task SearchRestaurantAsync_ReturnsRestaurantByAddress()
        {
            var context = GetInMemoryDbContext();
            await SeedData(context);
            var repository = new RestaurantRepository(context);

            var results = await repository.SearchRestaurantAsync(address: "Main St");

            Assert.Single(results);
            Assert.Equal("Italian Bistro", results[0].Name);
        }

        [Fact]
        public async Task SearchRestaurantAsync_ReturnsRestaurantByCategory()
        {
            var context = GetInMemoryDbContext();
            await SeedData(context);
            var repository = new RestaurantRepository(context);

            var results = await repository.SearchRestaurantAsync(category: "Italian");

            Assert.Single(results);
            Assert.Equal("Italian Bistro", results[0].Name);
        }

        [Fact]
        public async Task SearchRestaurantAsync_ReturnsEmptyListIfNoMatch()
        {
            var context = GetInMemoryDbContext();
            await SeedData(context);
            var repository = new RestaurantRepository(context);

            var results = await repository.SearchRestaurantAsync(name: "NonExistent");

            Assert.Empty(results);
        }

        [Fact]
        public async Task GetMenuAsync_ReturnsMenuItemsForValidRestaurant()
        {
            var context = GetInMemoryDbContext();
            await SeedData(context);
            var repository = new RestaurantRepository(context);

            var menu = await repository.GetMenuAsync(1);

            Assert.NotNull(menu);
            Assert.Equal(2, menu.Count);
            Assert.Contains(menu, item => item.Name == "Pasta");
        }

        [Fact]
        public async Task GetMenuAsync_ReturnsNullForRestaurantWithoutMenu()
        {
            var context = GetInMemoryDbContext();
            await SeedData(context);
            var repository = new RestaurantRepository(context);

            var menu = await repository.GetMenuAsync(3);

            Assert.Empty(menu);
        }

        [Fact]
        public async Task GetMenuAsync_ReturnsNullForNonExistentRestaurant()
        {
            var context = GetInMemoryDbContext();
            await SeedData(context);
            var repository = new RestaurantRepository(context);

            var menu = await repository.GetMenuAsync(999);

            Assert.Null(menu);
        }

        [Fact]
        public async Task GetRestaurantByIdAsync_ReturnsRestaurant_WhenIdExists()
        {
            var context = GetInMemoryDbContext();
            await SeedData(context);
            var repository = new RestaurantRepository(context);

            var restaurant = await repository.GetRestaurantByIdAsync(1);

            Assert.NotNull(restaurant);
            Assert.Equal("Italian Bistro", restaurant.Name);
            Assert.Equal("123 Main St", restaurant.Address);
            Assert.Equal("Italian", restaurant.Category);
        }

        [Fact]
        public async Task GetRestaurantByIdAsync_ReturnsNull_WhenIdDoesNotExist()
        {
            var context = GetInMemoryDbContext();
            await SeedData(context);
            var repository = new RestaurantRepository(context);

            var restaurant = await repository.GetRestaurantByIdAsync(99);

            Assert.Null(restaurant);
        }

        [Fact]
        public async Task GetMenuItemByIdAsync_ReturnsMenuItem_WhenIdExists()
        {
            var context = GetInMemoryDbContext();
            await SeedData(context);
            var repository = new RestaurantRepository(context);

            var menuItem = await repository.GetMenuItemByIdAsync(1);

            Assert.NotNull(menuItem);
            Assert.Equal("Pasta", menuItem.Name);
            Assert.Equal(12.99m, menuItem.Price);
        }

        [Fact]
        public async Task GetMenuItemByIdAsync_ReturnsNull_WhenIdDoesNotExist()
        {
            var context = GetInMemoryDbContext();
            await SeedData(context);
            var repository = new RestaurantRepository(context);

            var menuItem = await repository.GetMenuItemByIdAsync(99);

            Assert.Null(menuItem);
        }
    }
}
