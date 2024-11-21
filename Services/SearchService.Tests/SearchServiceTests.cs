using Microsoft.EntityFrameworkCore;
using SearchService.Data;
using SearchService.Models;
using SearchService.Repositories;

namespace SearchService.Tests
{
    public class SearchServiceTests
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
                    OpeningHours = "8:00 - 19:00",
                    Menu = new List<MenuItem>
                    {
                        new MenuItem { MenuItemID = 1, Name = "Pasta", Price = 12.99m },
                        new MenuItem { MenuItemID = 2, Name = "Pizza", Price = 15.49m }
                    },
                    Categories = new List<Categories>
                    {
                        new Categories {CategoryID = 1, Category = "Italian"},
                        new Categories {CategoryID = 2, Category = "Pizza"},
                        new Categories {CategoryID = 3, Category = "Pasta"}
                    }
                },
                new Restaurant
                {
                    RestaurantID = 2,
                    Name = "Sushi World",
                    Address = "456 Elm St",
                    OpeningHours = "10:00 - 17:00",
                    Menu = new List<MenuItem>
                    {
                        new MenuItem { MenuItemID = 3, Name = "Sushi Roll", Price = 8.99m },
                        new MenuItem { MenuItemID = 4, Name = "Tempura", Price = 10.99m }
                    },
                    Categories = new List<Categories>
                    {
                        new Categories {CategoryID = 4, Category = "Sushi"},
                        new Categories {CategoryID = 5, Category = "Fish"}
                    }
                },
                new Restaurant
                {
                    RestaurantID = 3,
                    Name = "Burger Haven",
                    Address = "789 Oak St",
                    OpeningHours = "12:00 - 23:00",
                    Menu = new List<MenuItem>(),
                    Categories = new List<Categories>()
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

            var results = await repository.SearchRestaurantAsync(category: "Pizza");

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

            if (menu == null)
            {
                Assert.Null(menu);
            }
            else
            {
                Assert.Empty(menu);
            }
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

        //Need to test all updates

        [Fact]
        public async Task AddMenuItemAsync_AddsMenuItemSuccessfully()
        {
            var context = GetInMemoryDbContext();
            await SeedData(context);
            var repository = new RestaurantRepository(context);

            var newMenuItem = new MenuItem { RestaurantID = 1, Name = "Garlic Bread", Price = 5.99m };

            var addedItem = await repository.AddMenuItemAsync(newMenuItem);

            Assert.NotNull(addedItem);
            Assert.Equal(newMenuItem.Name, addedItem.Name);
            Assert.Equal(newMenuItem.Price, addedItem.Price);
            Assert.Equal(newMenuItem.RestaurantID, addedItem.RestaurantID);

            var items = context.MenuItem.Where(m => m.RestaurantID == 1).ToList();
            Assert.Equal(3, items.Count);
            Assert.Contains(items, m => m.Name == "Garlic Bread");
        }

        [Fact]
        public async Task UpdateMenuItemAsync_UpdatesMenuItemSuccessfully()
        {
            var context = GetInMemoryDbContext();
            await SeedData(context);
            var repository = new RestaurantRepository(context);

            var updatedItem = await repository.UpdateMenuItemAsync(new MenuItem
            {
                MenuItemID = 1,
                Name = "Spaghetti Bolognese",
                Price = 14.99m
            });

            Assert.NotNull(updatedItem);
            Assert.Equal("Spaghetti Bolognese", updatedItem.Name);
            Assert.Equal(14.99m, updatedItem.Price);


            var menuItem = await context.MenuItem.FindAsync(1);
            Assert.NotNull(menuItem);
            Assert.Equal("Spaghetti Bolognese", menuItem.Name);
            Assert.Equal(14.99m, menuItem.Price);
        }

        [Fact]
        public async Task DeleteMenuItemAsync_DeletesMenuItemSuccessfully()
        {
            var context = GetInMemoryDbContext();
            await SeedData(context);
            var repository = new RestaurantRepository(context);

            var resultMessage = await repository.DeleteMenuItemAsync(1);

            Assert.NotNull(resultMessage);
            Assert.Equal("Deleted menuItem with id: 1", resultMessage);

            var menuItem = await context.MenuItem.FindAsync(1);
            Assert.Null(menuItem);
        }

        [Fact]
        public async Task AddRestaurantAsync_AddsRestaurantToDatabase()
        {
            var context = GetInMemoryDbContext();
            await SeedData(context);
            var repository = new RestaurantRepository(context);

            var newRestaurant = new Restaurant
            {
                Name = "New Restaurant",
                OpeningHours = "10:00 - 22:00",
                Address = "456 New St"
            };

            var addedRestaurant = await repository.AddRestaurantAsync(newRestaurant);

            Assert.NotNull(addedRestaurant);
            Assert.Equal("New Restaurant", addedRestaurant.Name);
            Assert.Equal("456 New St", addedRestaurant.Address);
            Assert.Equal("10:00 - 22:00", addedRestaurant.OpeningHours);

            var restaurants = await context.Restaurant.ToListAsync();
            Assert.Equal(4, restaurants.Count);
            Assert.Contains(restaurants, r => r.Name == "New Restaurant");
        }

        [Fact]
        public async Task UpdateRestaurantAsync_UpdatesRestaurantSuccessfully()
        {
            var context = GetInMemoryDbContext();
            await SeedData(context);
            var repository = new RestaurantRepository(context);

            var updatedRestaurant = await repository.UpdateRestaurantAsync(new Restaurant
            {
                RestaurantID = 1,
                Name = "Updated Italian Bistro",
                OpeningHours = "11:00 - 23:00",
                Address = "Updated Address"
            });

            Assert.NotNull(updatedRestaurant);
            Assert.Equal("Updated Italian Bistro", updatedRestaurant.Name);
            Assert.Equal("Updated Address", updatedRestaurant.Address);
            Assert.Equal("11:00 - 23:00", updatedRestaurant.OpeningHours);

            var restaurant = await context.Restaurant.FindAsync(1);
            Assert.NotNull(restaurant);
            Assert.Equal("Updated Italian Bistro", restaurant.Name);
            Assert.Equal("Updated Address", restaurant.Address);
            Assert.Equal("11:00 - 23:00", restaurant.OpeningHours);
        }

        [Fact]
        public async Task DeleteRestaurantAsync_DeletesRestaurantAndAssociatedData()
        {
            var context = GetInMemoryDbContext();
            await SeedData(context);
            var repository = new RestaurantRepository(context);

            var result = await repository.DeleteRestaurantAsync(1);

            Assert.NotNull(result);
            Assert.Equal("Deleted restaurant and all related items with id: 1", result);

            var deletedRestaurant = await context.Restaurant.FindAsync(1);
            var menuItems = context.MenuItem.Where(m => m.RestaurantID == 1).ToList();
            var categories = context.Categories.Where(c => c.RestaurantID == 1).ToList();

            Assert.Null(deletedRestaurant);
            Assert.Empty(menuItems);
            Assert.Empty(categories);
        }


        [Fact]
        public async Task GetCategoryByIdAsync_ReturnsCategory_WhenIdExists()
        {
            var context = GetInMemoryDbContext();
            await SeedData(context);
            var repository = new RestaurantRepository(context);

            var category = await repository.GetCategoryByIdAsync(1);

            Assert.NotNull(category);
            Assert.Equal("Italian", category.Category);
        }

        [Fact]
        public async Task GetCategoryByIdAsync_ReturnsNull_WhenIdDoesNotExist()
        {
            var context = GetInMemoryDbContext();
            await SeedData(context);
            var repository = new RestaurantRepository(context);

            var category = await repository.GetCategoryByIdAsync(99);

            Assert.Null(category);
        }

        [Fact]
        public async Task AddCategoryAsync_AddsCategoryToDatabase()
        {
            var context = GetInMemoryDbContext();
            await SeedData(context);
            var repository = new RestaurantRepository(context);

            var newCategory = new Categories
            {
                RestaurantID = 1,
                Category = "Vegan"
            };

            var addedCategory = await repository.AddCategoryAsync(newCategory);

            Assert.NotNull(addedCategory);
            Assert.Equal("Vegan", addedCategory.Category);
            Assert.Equal(1, addedCategory.RestaurantID);

            var categories = await context.Categories.ToListAsync();
            Assert.Equal(6, categories.Count);
            Assert.Contains(categories, c => c.Category == "Vegan");
        }

        [Fact]
        public async Task UpdateCategoryAsync_UpdatesCategorySuccessfully()
        {
            var context = GetInMemoryDbContext();
            await SeedData(context);
            var repository = new RestaurantRepository(context);

            var updatedCategory = await repository.UpdateCategoryAsync(new Categories
            {
                CategoryID = 1,
                RestaurantID = 1,
                Category = "Updated Italian"
            });

            Assert.NotNull(updatedCategory);
            Assert.Equal("Updated Italian", updatedCategory.Category);
            Assert.Equal(1, updatedCategory.RestaurantID);

            var category = await context.Categories.FindAsync(1);
            Assert.NotNull(category);
            Assert.Equal("Updated Italian", category.Category);
        }

        [Fact]
        public async Task DeleteCategoryAsync_DeletesCategorySuccessfully()
        {
            var context = GetInMemoryDbContext();
            await SeedData(context);
            var repository = new RestaurantRepository(context);

            var deletedCategoryMessage = await repository.DeleteCategoryAsync(1);

            Assert.NotNull(deletedCategoryMessage);
            Assert.Equal("Deleted category with id: 1", deletedCategoryMessage);

            var categories = await context.Categories.ToListAsync();
            Assert.Equal(4, categories.Count);
            Assert.DoesNotContain(categories, c => c.CategoryID == 1);

        }
    }
}
