using System;

class Program
{
    static void Main(string[] args)
    {
        var rabbitMqHost = "localhost";
        var queueName = "foodDeliveryAppQueue";
        var httpClient = new HttpClient();

        var notificationService = new NotificationService(rabbitMqHost, queueName, httpClient);

        notificationService.StartListening();

        Console.WriteLine("Notification service is now listening for messages...");
        Console.ReadLine();
        notificationService.Dispose();
    }
}
