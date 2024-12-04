using System.ComponentModel.DataAnnotations;

namespace FeedbackService.Models
{
    public class Feedback
    {
        [Key]
        public int FeedbackID { get; set; }

        public double FoodRating { get; set; }

        public double DeliveryRating { get; set; }

        [MaxLength(1000)]
        public string Comment { get; set; } = string.Empty;

        public int RestaurantID { get; set; }

        public int DeliveryDriverID { get; set; }
    }
}