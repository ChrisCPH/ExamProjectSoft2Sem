using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace OrderService.Models
{
    public class Order
    {
        [Key]
        public int OrderID { get; set; }

        public int CustomerID { get; set; }

        public int RestaurantID { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice {get; set; }

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "Pending"; // "Pending", "Placed", "Ready For Pickup", "Accepted", "Delivered"

        public int DriverID { get; set; }
    }
}