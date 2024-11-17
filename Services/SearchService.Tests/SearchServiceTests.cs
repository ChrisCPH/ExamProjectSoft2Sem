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
            var restaurants = new List<Restaurant>
            {
                new Restaurant
                {
                    Id = 1,
                    Name = "Italian Bistro",
                    Address = "123 Main St",
                    Category = "Italian",
                    Menu = new List<MenuItem>
                    {
                        new MenuItem { Id = 1, Name = "Pasta", Price = 12.99m },
                        new MenuItem { Id = 2, Name = "Pizza", Price = 15.49m }
                    }
                },
                new Restaurant
                {
                    Id = 2,
                    Name = "Sushi World",
                    Address = "456 Elm St",
                    Category = "Japanese",
                    Menu = new List<MenuItem>
                    {
                        new MenuItem { Id = 3, Name = "Sushi Roll", Price = 8.99m },
                        new MenuItem { Id = 4, Name = "Tempura", Price = 10.99m }
                    }
                },
                new Restaurant
                {
                    Id = 3,
                    Name = "Burger Haven",
                    Address = "789 Oak St",
                    Category = "American",
                    Menu = null
                }
            };

            context.Restaurants.AddRange(restaurants);
            await context.SaveChangesAsync();
        }

        [Fact]
        public async Task SearchRestaurantsAsync_ReturnsRestaurantsByName()
        {
            var context = GetInMemoryDbContext();
            await SeedData(context);
            var repository = new RestaurantRepository(context);

            var results = await repository.SearchRestaurantsAsync(name: "Sushi");

            Assert.Single(results);
            Assert.Equal("Sushi World", results[0].Name);
        }

        [Fact]
        public async Task SearchRestaurantsAsync_ReturnsRestaurantsByAddress()
        {
            var context = GetInMemoryDbContext();
            await SeedData(context);
            var repository = new RestaurantRepository(context);

            var results = await repository.SearchRestaurantsAsync(address: "Main St");

            Assert.Single(results);
            Assert.Equal("Italian Bistro", results[0].Name);
        }

        [Fact]
        public async Task SearchRestaurantsAsync_ReturnsRestaurantsByCategory()
        {
            var context = GetInMemoryDbContext();
            await SeedData(context);
            var repository = new RestaurantRepository(context);

            var results = await repository.SearchRestaurantsAsync(category: "Italian");

            Assert.Single(results);
            Assert.Equal("Italian Bistro", results[0].Name);
        }

        [Fact]
        public async Task SearchRestaurantsAsync_ReturnsEmptyListIfNoMatch()
        {
            var context = GetInMemoryDbContext();
            await SeedData(context);
            var repository = new RestaurantRepository(context);

            var results = await repository.SearchRestaurantsAsync(name: "NonExistent");

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
    }
}
