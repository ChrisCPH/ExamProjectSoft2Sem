using Microsoft.AspNetCore.Mvc;
using SearchService.Repositories;
using System.Threading.Tasks;

namespace SearchService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RestaurantController : ControllerBase
    {
        private readonly RestaurantRepository _repository;

        public RestaurantController(RestaurantRepository repository)
        {
            _repository = repository;
        }

        // GET: api/restaurant/search?name=Pizza&address=Downtown&category=Italian
        [HttpGet("search")]
        public async Task<IActionResult> SearchRestaurant([FromQuery] string name, [FromQuery] string address, [FromQuery] string category)
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
    }
}
