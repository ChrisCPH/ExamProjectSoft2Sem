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
            IQueryable<Restaurant> query = _context.Restaurant.Include(r => r.Menu);

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
                query = query.Where(r => r.Category.Contains(category));
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
                .FirstOrDefaultAsync(r => r.RestaurantID == restaurantID);
        }

        public async Task<MenuItem?> GetMenuItemByIdAsync(int menuItemID)
        {
            return await _context.MenuItem
                .FirstOrDefaultAsync(m => m.MenuItemID == menuItemID);
        }
    }
}
