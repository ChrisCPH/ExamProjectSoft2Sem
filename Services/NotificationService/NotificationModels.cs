using System;

namespace NotificationModels
{
    public class OrderMessage
    {
        public int OrderID { get; set; }
        public int CustomerID { get; set; }
        public int RestaurantID { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } = string.Empty;
        public int DriverID { get; set; }
        public List<OrderItemMessage>? OrderItems { get; set; }
    }

    public class FeeMessage
    {
        public int FeeID { get; set; }
        public decimal Amount { get; set; }
        public int OrderCount { get; set; }
        public decimal TotalOrderPrice { get; set; }
        public int RestaurantID { get; set; }
        public DateTime InvoiceDate { get; set; }
        public DateTime DueDate { get; set; }
        public string Status { get; set; } = string.Empty; // Pending, Paid
    }

    public class PaymentMessage
    {
        public int PaymentID { get; set; }
        public decimal Amount { get; set; }
        public int DeliveryCount { get; set; }
        public decimal TotalDeliveryPrice { get; set; }
        public int DeliveryDriverID { get; set; }
        public DateTime PaycheckDate { get; set; }
        public string Status { get; set; } = string.Empty; // Pending, Paid   
    }

    public class OrderItemMessage
    {
        public int OrderItemID { get; set; }
        public int MenuItemID { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
    }

    public class AccountInfo
    {
        public int AccountID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public AccountType AccountType { get; set; }
        public string Password { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string Address { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string PaymentInfo { get; set; } = string.Empty;
        public double AvgRating { get; set; }
        public int RestaurantSearchID { get; set; }
        public DateTime StatusChanged { get; set; }
    }

    public class MenuItemInfo
    {
        public int MenuItemID { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string? Description { get; set; }
        public int RestaurantID { get; set; }
    }

    public enum AccountType
    {
        Customer,
        Restaurant,
        DeliveryDriver,
        Admin
    }
}