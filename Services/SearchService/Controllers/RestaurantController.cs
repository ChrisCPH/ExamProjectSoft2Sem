using Microsoft.AspNetCore.Mvc;
using SearchService.Repositories;
using System.Threading.Tasks;
using SearchService.Models;

namespace SearchService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RestaurantController : ControllerBase
    {
        private readonly IRestaurantRepository _repository;

        public RestaurantController(IRestaurantRepository repository)
        {
            _repository = repository;
        }

        // GET: api/restaurant/search?name=&address=&categories=
        [HttpGet("search")]
        public async Task<IActionResult> SearchRestaurant([FromQuery] string? name, [FromQuery] string? address, [FromQuery] string? category)
        {
            var results = await _repository.SearchRestaurantAsync(name, address, category);
            return Ok(results);
        }

        // GET: api/restaurant/{id}/menu
        [HttpGet("{id}/menu")]
        public async Task<IActionResult> GetMenu(int id)
        {
            var menu = await _repository.GetMenuAsync(id);
            if (menu == null)
            {
                return NotFound();
            }
            return Ok(menu);
        }

        // GET: api/restaurant/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetRestaurantById(int id)
        {
            var restaurant = await _repository.GetRestaurantByIdAsync(id);

            if (restaurant == null)
            {
                return NotFound(new { Message = $"Restaurant with ID {id} not found." });
            }

            return Ok(restaurant);
        }

        // GET: api/restaurant/menu/{id}
        [HttpGet("menu/{id}")]
        public async Task<IActionResult> GetMenuItemById(int id)
        {
            var menuItem = await _repository.GetMenuItemByIdAsync(id);
            if (menuItem == null)
            {
                return NotFound();
            }

            return Ok(menuItem);
        }

        // GET: api/restaurant/category/{id}
        [HttpGet("category/{id}")]
        public async Task<IActionResult> GetCategoryById(int id)
        {
            var category = await _repository.GetCategoryByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            return Ok(category);
        }

        [HttpPost("menu")]
        public async Task<IActionResult> AddMenuItemAsync([FromBody] MenuItem menuItem)
        {
            var createdItem = await _repository.AddMenuItemAsync(menuItem);
            return Ok(createdItem);
        }

        [HttpPut("menu")]
        public async Task<IActionResult> UpdateMenuItemAsync([FromBody] MenuItem menuItem)
        {
            var updated = await _repository.UpdateMenuItemAsync(menuItem);

            if (updated == null)
            {
                return NotFound();
            }

            return Ok(updated);
        }

        [HttpDelete("menu/{menuItemId}")]
        public async Task<IActionResult> DeleteMenuItemAsync(int menuItemId)
        {
            var deleted = await _repository.DeleteMenuItemAsync(menuItemId);
            if (deleted == null)
            {
                return NotFound();
            }
            return Ok(new { Message = deleted });
        }

        [HttpPost]
        public async Task<IActionResult> AddRestaurantAsync([FromBody] Restaurant restaurant)
        {
            var addedRestaurant = await _repository.AddRestaurantAsync(restaurant);
            return Ok(addedRestaurant);
        }

        [HttpDelete("{restaurantId}")]
        public async Task<IActionResult> DeleteRestaurantAsync(int restaurantId)
        {
            var deleted = await _repository.DeleteRestaurantAsync(restaurantId);
            if (deleted == null)
            {
                return NotFound();
            }
            return Ok(new { Message = deleted });
        }

        [HttpPut]
        public async Task<IActionResult> UpdateRestaurantAsync([FromBody] Restaurant updatedRestaurant)
        {
            var updated = await _repository.UpdateRestaurantAsync(updatedRestaurant);

            if (updated == null)
            {
                return NotFound();
            }

            return Ok(updated);
        }

        [HttpPost("categories")]
        public async Task<IActionResult> AddCategoryAsync([FromBody] Categories category)
        {
            var addedCategory = await _repository.AddCategoryAsync(category);
            return Ok(addedCategory);
        }

        [HttpDelete("categories/{categoryId}")]
        public async Task<IActionResult> DeleteCategoryAsync(int categoryId)
        {
            var deleted = await _repository.DeleteCategoryAsync(categoryId);
            if (deleted == null)
            {
                return NotFound();
            }
            return Ok(new { Message = deleted });
        }

        [HttpPut("categories")]
        public async Task<IActionResult> UpdateCategoryAsync([FromBody] Categories updatedCategory)
        {
            var updated = await _repository.UpdateCategoryAsync(updatedCategory);

            if (updated == null)
            {
                return NotFound();
            }

            return Ok(updated);
        }
    }
}
