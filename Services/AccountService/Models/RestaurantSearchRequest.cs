namespace AccountService.Models
{
    public class RestaurantSearchRequest
    {
        public int restaurantID { get; set; }
        public string name { get; set; } = string.Empty;
        public string address { get; set; } = string.Empty;
        public string category { get; set; } = string.Empty;
    }
}