using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ManagementService.Models
{
    public class Fee 
    {
        public int FeeID { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public int OrderCount { get; set; }  

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalOrderPrice { get; set; }

        public int RestaurantID { get; set; }

        public DateTime InvoiceDate { get; set; }

        public DateTime DueDate { get; set; }

        public DateTime PaidDate { get; set; } = new DateTime(2000, 1, 1);

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = string.Empty; // Pending, Paid
    }
}