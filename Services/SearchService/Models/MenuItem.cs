using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace SearchService.Models
{
    public class MenuItem
    {
        [Key]
        public int MenuItemID { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [MaxLength(255)]
        public string? Description { get; set; }

        [ForeignKey("Restaurant")]
        public int RestaurantID { get; set; }

        [JsonIgnore]
        public Restaurant? Restaurant { get; set; }
    }
}
