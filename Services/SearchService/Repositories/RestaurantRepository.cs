using Microsoft.EntityFrameworkCore;
using SearchService.Models;
using SearchService.Data;

namespace SearchService.Repositories
{
    public class RestaurantRepository
    {
        private readonly SearchDbContext _context;

        public RestaurantRepository(SearchDbContext context)
        {
            _context = context;
        }

        public async Task<List<Restaurant>> SearchRestaurantAsync(string? name = null, string? address = null, string? category = null)
        {
            IQueryable<Restaurant> query = _context.Restaurant
                .Include(r => r.Menu)
                .Include(r => r.Categories);

            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(r => r.Name.Contains(name));
            }

            if (!string.IsNullOrEmpty(address))
            {
                query = query.Where(r => r.Address.Contains(address));
            }

            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(r => r.Categories.Any(c => c.Category.Contains(category)));
            }

            return await query.ToListAsync();
        }

        public async Task<List<MenuItem>?> GetMenuAsync(int restaurantID)
        {
            var restaurant = await _context.Restaurant
                .Include(r => r.Menu)
                .FirstOrDefaultAsync(r => r.RestaurantID == restaurantID);

            if (restaurant == null)
            {
                return null;
            }

            return restaurant?.Menu;
        }

        public async Task<Restaurant?> GetRestaurantByIdAsync(int restaurantID)
        {
            return await _context.Restaurant
                .Include(r => r.Menu)
                .Include(r => r.Categories)
                .FirstOrDefaultAsync(r => r.RestaurantID == restaurantID);
        }

        public async Task<MenuItem?> GetMenuItemByIdAsync(int menuItemID)
        {
            return await _context.MenuItem.FirstOrDefaultAsync(m => m.MenuItemID == menuItemID);
        }

        public async Task<Categories?> GetCategoryByIdAsync(int categoryId)
        {
            return await _context.Categories.FirstOrDefaultAsync(c => c.CategoryID == categoryId);
        }

        public async Task<MenuItem> AddMenuItemAsync(MenuItem menuItem)
        {
            _context.MenuItem.Add(menuItem);
            await _context.SaveChangesAsync();
            return menuItem;
        }

        public async Task<MenuItem?> UpdateMenuItemAsync(MenuItem menuItem)
        {
            var existingItem = await _context.MenuItem.FirstOrDefaultAsync(m => m.MenuItemID == menuItem.MenuItemID);
            if (existingItem == null)
            {
                return null;
            }

            existingItem.Name = menuItem.Name;
            existingItem.Price = menuItem.Price;
            existingItem.Description = menuItem.Description;
            await _context.SaveChangesAsync();

            return existingItem;
        }

        public async Task<string?> DeleteMenuItemAsync(int menuItemId)
        {
            var menuItem = await _context.MenuItem.FirstOrDefaultAsync(m => m.MenuItemID == menuItemId);

            if (menuItem == null)
            {
                return null;
            }

            _context.MenuItem.Remove(menuItem);
            await _context.SaveChangesAsync();

            return $"Deleted menuItem with id: {menuItem.MenuItemID}";
        }

        public async Task<Restaurant> AddRestaurantAsync(Restaurant restaurant)
        {
            _context.Restaurant.Add(restaurant);
            await _context.SaveChangesAsync();
            return restaurant;
        }

        public async Task<string?> DeleteRestaurantAsync(int restaurantId)
        {
            var restaurant = await _context.Restaurant
                .Include(r => r.Menu)
                .Include(r => r.Categories)
                .FirstOrDefaultAsync(r => r.RestaurantID == restaurantId);

            if (restaurant == null)
            {
                return null;
            }

            _context.MenuItem.RemoveRange(restaurant.Menu);

            _context.Categories.RemoveRange(restaurant.Categories);

            _context.Restaurant.Remove(restaurant);

            await _context.SaveChangesAsync();

            return $"Deleted restaurant and all related items with id: {restaurant.RestaurantID}";
        }

        public async Task<Restaurant?> UpdateRestaurantAsync(Restaurant restaurant)
        {
            var existingRestaurant = await _context.Restaurant.FirstOrDefaultAsync(r => r.RestaurantID == restaurant.RestaurantID);

            if (existingRestaurant == null)
            {
                return null;
            }

            existingRestaurant.Name = restaurant.Name;
            existingRestaurant.OpeningHours = restaurant.OpeningHours;
            existingRestaurant.Address = restaurant.Address;

            await _context.SaveChangesAsync();
            return existingRestaurant;
        }

        public async Task<Categories> AddCategoryAsync(Categories category)
        {
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return category;
        }

        public async Task<string?> DeleteCategoryAsync(int categoryId)
        {
            var category = await _context.Categories.FindAsync(categoryId);

            if (category == null)
            {
                return null;
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return $"Deleted category with id: {category.CategoryID}";
        }

        public async Task<Categories?> UpdateCategoryAsync(Categories category)
        {
            var existingCategory = await _context.Categories.FirstOrDefaultAsync(c => c.CategoryID == category.CategoryID);

            if (existingCategory == null)
            {
                return null;
            }

            existingCategory.Category = category.Category;

            await _context.SaveChangesAsync();
            return existingCategory;
        }
    }
}
