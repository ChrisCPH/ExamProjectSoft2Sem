using System;
using System.ComponentModel.DataAnnotations;

namespace AccountService.Models
{
    public class Account
    {
        [Key]
        public int AccountID { get; set; }

        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;

        [MaxLength(15)]
        public string PhoneNumber { get; set; } = string.Empty;

        public AccountType AccountType { get; set; } // "Customer", "Restaurant", "DeliveryDriver", "Admin"

        [Required]
        [MaxLength(int.MaxValue)]
        public string Password { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Language { get; set; } = string.Empty; // "English" , "Danish"

        public DateTime CreatedAt { get; set; }

        [MaxLength(255)]
        public string Address { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Status { get; set; } = string.Empty;

        [MaxLength(255)]
        public string PaymentInfo { get; set; } = string.Empty;

        public double AvgRating { get; set; }

        public int RestaurantSearchID { get; set; }
    }

    public enum AccountType
    {
        Customer,
        Restaurant,
        DeliveryDriver,
        Admin
    }
}