using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SearchService.Models
{
    public class Categories
    {
        [Key]
        public int CategoryID { get; set; }

        public int RestaurantID { get; set; }

        [Required]
        [MaxLength(255)]
        public string Category { get; set; } = string.Empty;
    }
}
