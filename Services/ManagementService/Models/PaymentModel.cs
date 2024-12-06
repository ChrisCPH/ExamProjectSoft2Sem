using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ManagementService.Models
{
    public class Payment
    {
        public int PaymentID { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public int DeliveryCount { get; set; }  

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalDeliveryPrice { get; set; }

        public int DeliveryDriverID { get; set; }

        public DateTime PaycheckDate { get; set; }

        public DateTime PaidDate { get; set; } = new DateTime(2000, 1, 1);

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = string.Empty; // Pending, Paid            
    }
}