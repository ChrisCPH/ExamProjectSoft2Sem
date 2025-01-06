using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NotificationModels;
using Newtonsoft.Json.Linq;
using System.Net.Mail;
using System.Net;

public class NotificationService : IDisposable
{
    private readonly string _rabbitMqHost;
    private readonly string _queueName;
    private IConnection _connection;
    private IModel _channel;
    private readonly HttpClient _httpClient;
    private readonly string _smtpHost = "smtp.gmail.com";
    private readonly int _smtpPort = 587;
    private readonly string _email = ""; // Sender email
    private readonly string _emailPassword = ""; // App password

    public NotificationService(string rabbitMqHost, string queueName, HttpClient httpClient)
    {
        _rabbitMqHost = rabbitMqHost;
        _queueName = queueName;
        _httpClient = httpClient;

        var factory = new ConnectionFactory() { HostName = _rabbitMqHost };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.QueueDeclare(queue: _queueName, durable: true, exclusive: false, autoDelete: false);
    }

    public void StartListening()
    {
        try
        {
            if (_connection == null || !_connection.IsOpen || _channel == null || !_channel.IsOpen)
            {
                Console.WriteLine("Connection or channel is not properly initialized.");
                return;
            }

            _channel.QueueDeclare(queue: _queueName, durable: true, exclusive: false, autoDelete: false);

            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += async (sender, args) =>
            {
                try
                {
                    var body = args.Body.ToArray();
                    var messageJson = Encoding.UTF8.GetString(body);

                    var messageObject = JObject.Parse(messageJson);

                    var messageTypeToken = messageObject["Type"];
                    if (messageTypeToken == null)
                    {
                        Console.WriteLine("Message does not contain a 'Type' field.");
                        return;
                    }

                    var messageType = messageTypeToken.ToString();

                    var message = JsonConvert.DeserializeObject<OrderMessage>(messageJson);

                    if (messageType.Contains("Order", StringComparison.OrdinalIgnoreCase))
                    {
                        var orderMessage = JsonConvert.DeserializeObject<OrderMessage>(messageJson);
                        if (orderMessage != null)
                        {
                            await SendOrderNotification(orderMessage, messageType);
                        }
                        else
                        {
                            Console.WriteLine("Failed to deserialize OrderMessage.");
                        }
                    }
                    else if (messageType.Contains("Fee", StringComparison.OrdinalIgnoreCase))
                    {
                        var feeMessage = JsonConvert.DeserializeObject<FeeMessage>(messageJson);
                        if (feeMessage != null)
                        {
                            await SendFeeNotification(feeMessage);
                        }
                        else
                        {
                            Console.WriteLine("Failed to deserialize FeePaymentMessage.");
                        }
                    }
                    else if (messageType.Contains("Payment", StringComparison.OrdinalIgnoreCase))
                    {
                        var paymentMessage = JsonConvert.DeserializeObject<PaymentMessage>(messageJson);
                        if (paymentMessage != null)
                        {
                            await SendPaymentNotification(paymentMessage);
                        }
                        else
                        {
                            Console.WriteLine("Failed to deserialize DriverPaymentMessage.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Failed to deserialize the message.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing message: {ex.Message}");
                }
            };

            _channel.BasicConsume(queue: _queueName, autoAck: true, consumer: consumer);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error starting listener: {ex.Message}");
        }
    }

    private async Task SendOrderNotification(OrderMessage message, string messageType)
    {
        if (message == null)
        {
            Console.WriteLine("Received a null message. Skipping processing.");
            return;
        }

        var customerInfo = await GetAccountInfoAsync(message.CustomerID);
        if (customerInfo == null)
        {
            Console.WriteLine("Customer not found");
            return;
        }

        var restaurantInfo = await GetAccountInfoAsync(message.RestaurantID);
        if (restaurantInfo == null)
        {
            Console.WriteLine("Restaurant not found");
            return;
        }

        var menuItemDetails = await GetMenuItemDetails(message);

        switch (messageType)
        {
            case "OrderPlaced":

                var customerEmailSubject = "Order Placed";
                var customerEmailBody = @$"Order ID: {message.OrderID}, 
                                        Restaurant: {restaurantInfo.Name}, 
                                        Price: {message.TotalPrice} DKK, 
                                        Order Details: {menuItemDetails}";

                await SendEmail(customerInfo.Email, customerEmailSubject, customerEmailBody);

                var CustomerSms = $"Your order #{message.OrderID} has been placed successfully!";
                MockSendSms(customerInfo.PhoneNumber, CustomerSms);

                var restaurantEmailSubject = "Order Placed";
                var restaurantEmailBody = @$"Order ID: {message.OrderID}, 
                                        Customer: {customerInfo.Name}, 
                                        Price: {message.TotalPrice} DKK, 
                                        Order Details: {menuItemDetails}";

                await SendEmail(restaurantInfo.Email, restaurantEmailSubject, restaurantEmailBody);

                var RestaurantSms = $"A new order has been place #{message.OrderID}";
                MockSendSms(restaurantInfo.PhoneNumber, RestaurantSms);

                break;

            case "OrderReady":

                if (message.DriverID != 0)
                {
                    var driverInfo = await GetAccountInfoAsync(message.DriverID);
                    if (driverInfo != null)
                    {

                        var driverEmailSubject = "Order Ready";
                        var driverEmailBody = @$"Restaurant Address: {restaurantInfo.Address}, 
                                        Customer Address: {customerInfo.Address}, 
                                        Order ID: {message.OrderID}, 
                                        Order Details: {menuItemDetails}";

                        await SendEmail(driverInfo.Email, driverEmailSubject, driverEmailBody);

                        var orderReadySms = $"Order #{message.OrderID} is ready for pickup at {restaurantInfo.Address}.";
                        MockSendSms(driverInfo.PhoneNumber, orderReadySms);
                    }
                    else
                    {
                        Console.WriteLine("Driver not found");
                    }
                }
                break;

            case "OrderDelivered":

                var deliveredEmailSubject = "Order Delivered";
                var deliveredEmailBody = @$"Order ID: {message.OrderID}, 
                                        Price: {message.TotalPrice} DKK, 
                                        Order Details: {menuItemDetails}";

                await SendEmail(customerInfo.Email, deliveredEmailSubject, deliveredEmailBody);

                var feedbackSms = $"Your order #{message.OrderID} has been delivered! Please leave feedback.";
                MockSendSms(customerInfo.PhoneNumber, feedbackSms);
                break;

            default:
                Console.WriteLine($"Unknown message type: {messageType}");
                break;
        }
    }

    private async Task SendFeeNotification(FeeMessage message)
    {
        if (message == null)
        {
            Console.WriteLine("Received a null message. Skipping processing.");
            return;
        }

        var restaurantInfo = await GetAccountInfoAsync(message.RestaurantID);
        if (restaurantInfo == null)
        {
            Console.WriteLine("Restaurant not found");
            return;
        }

        var emailSubject = "Fee Invoice";
        var emailBody = @$"Fee ID: {message.FeeID}, 
                        Amount: {message.Amount} DKK, 
                        Restaurant: {restaurantInfo.Name}, 
                        Address: {restaurantInfo.Address}, 
                        Total price of orders: {message.TotalOrderPrice} DKK, 
                        Total orders: {message.OrderCount}, 
                        Invoice date: {message.InvoiceDate}, 
                        Due date: {message.DueDate}";

        await SendEmail(restaurantInfo.Email, emailSubject, emailBody);

        var sms = $"Your order #{message.FeeID} has been delivered! Please leave feedback.";
        MockSendSms(restaurantInfo.PhoneNumber, sms);
    }

    private async Task SendPaymentNotification(PaymentMessage message)
    {
        if (message == null)
        {
            Console.WriteLine("Received a null message. Skipping processing.");
            return;
        }

        var driverInfo = await GetAccountInfoAsync(message.DeliveryDriverID);
        if (driverInfo == null)
        {
            Console.WriteLine("Delivery Driver not found");
            return;
        }

        var emailSubject = "Paycheck";
        var emailBody = @$"Payment ID: {message.PaymentID}, 
                        Amount: {message.Amount} DKK, 
                        Delivery Driver: {driverInfo.Name}, 
                        Address: {driverInfo.Address}, 
                        Total price of deliveries: {message.TotalDeliveryPrice} DKK, 
                        Total deliveries: {message.DeliveryCount}, 
                        Date: {message.PaycheckDate}";

        await SendEmail(driverInfo.Email, emailSubject, emailBody);

        var sms = $"Your order #{message.PaymentID} has been delivered! Please leave feedback.";
        MockSendSms(driverInfo.PhoneNumber, sms);
    }

    public async Task SendEmail(string toEmail, string subject, string body)
    {
        //toEmail = ""; // Use this when testing
        try
        {
            var mailMessage = new MailMessage
            {
                From = new MailAddress(_email),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mailMessage.To.Add(toEmail);

            using (var smtpClient = new SmtpClient(_smtpHost, _smtpPort))
            {
                smtpClient.Credentials = new NetworkCredential(_email, _emailPassword);
                smtpClient.EnableSsl = true;
                await smtpClient.SendMailAsync(mailMessage);
                Console.WriteLine($"Email sent successfully to {toEmail}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to send email: {ex.Message}");
        }
    }

    private void MockSendSms(string phoneNumber, string message)
    {
        Console.WriteLine($"[Mock SMS] To: {phoneNumber}, Message: {message}");
    }

    public async Task<AccountInfo?> GetAccountInfoAsync(int accountId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"http://localhost:5290/api/account/{accountId}");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var accountInfo = JsonConvert.DeserializeObject<AccountInfo>(content);

                if (accountInfo == null)
                {
                    Console.WriteLine("Deserialized account info is null.");
                    return null;
                }

                return accountInfo;
            }
            else
            {
                Console.WriteLine($"Failed to fetch account info. Status code: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching account info: {ex.Message}");
        }

        return null;
    }

    public async Task<MenuItemInfo?> GetMenuItemInfo(int menuItemId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"http://localhost:5045/api/restaurant/menu/{menuItemId}");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var menuItemInfo = JsonConvert.DeserializeObject<MenuItemInfo>(content);

                if (menuItemInfo == null)
                {
                    Console.WriteLine("Deserialized menu item info is null.");
                    return null;
                }

                return menuItemInfo;
            }
            else
            {
                Console.WriteLine($"Failed to fetch menu item info. Status code: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching menu item info: {ex.Message}");
        }

        return null;
    }

    public async Task<string> GetMenuItemDetails(OrderMessage message)
    {
        if (message.OrderItems == null || !message.OrderItems.Any())
        {
            return "No order items found.";
        }

        var menuItems = new List<MenuItemInfo>();

        foreach (var item in message.OrderItems)
        {
            var menuItemInfo = await GetMenuItemInfo(item.MenuItemID);
            if (menuItemInfo == null)
            {
                Console.WriteLine($"Menu item with ID {item.MenuItemID} not found.");
                return $"Menu item with ID {item.MenuItemID} not found.";
            }
            menuItems.Add(menuItemInfo);
        }

        var menuItemDetails = string.Join(", ", message.OrderItems
            .Select(orderItem =>
            {
                var menuItemInfo = menuItems.FirstOrDefault(m => m.MenuItemID == orderItem.MenuItemID);
                return menuItemInfo != null ? $"{menuItemInfo.Name} (x{orderItem.Quantity})" : null;
            })
            .Where(detail => detail != null));

        return menuItemDetails;
    }


    public void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
    }
}
