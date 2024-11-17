using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SearchService.Models
{
    public class Restaurant
    {
        [Key]
        public int RestaurantID { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string Category { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Address { get; set; } = string.Empty;

        public List<MenuItem> Menu { get; set; } = new List<MenuItem>();
    }
}
