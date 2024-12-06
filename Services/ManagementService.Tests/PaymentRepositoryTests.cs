using ManagementService.Models;
using ManagementService.Data;
using ManagementService.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

public class PaymentRepositoryTests
{
    private ManagementDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ManagementDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new ManagementDbContext(options);
    }

    [Fact]
    public async Task GetAllPayments_ShouldReturnAllPayments()
    {
        var context = GetInMemoryDbContext();
        var repository = new PaymentRepository(context);

        var payments = new List<Payment>
        {
            new Payment { PaymentID = 1, Amount = 50.00m, DeliveryCount = 10, TotalDeliveryPrice = 200.00m, DeliveryDriverID = 201, PaycheckDate = DateTime.Now, Status = "Pending" },
            new Payment { PaymentID = 2, Amount = 75.00m, DeliveryCount = 15, TotalDeliveryPrice = 300.00m, DeliveryDriverID = 202, PaycheckDate = DateTime.Now, Status = "Paid" }
        };

        await context.Payment.AddRangeAsync(payments);
        await context.SaveChangesAsync();

        var result = await repository.GetAllPayments();

        Assert.Equal(2, result.Count);
        Assert.Contains(result, p => p.PaymentID == 1 && p.Amount == 50.00m);
        Assert.Contains(result, p => p.PaymentID == 2 && p.Status == "Paid");
    }

    [Fact]
    public async Task GetPaymentById_ShouldReturnCorrectPayment()
    {
        var context = GetInMemoryDbContext();
        var repository = new PaymentRepository(context);

        var payment = new Payment { PaymentID = 1, Amount = 50.00m, DeliveryCount = 10, TotalDeliveryPrice = 200.00m, DeliveryDriverID = 201, PaycheckDate = DateTime.Now, Status = "Pending" };
        await context.Payment.AddAsync(payment);
        await context.SaveChangesAsync();

        var result = await repository.GetPaymentById(1);

        Assert.NotNull(result);
        Assert.Equal(50.00m, result?.Amount);
        Assert.Equal("Pending", result?.Status);
    }

    [Fact]
    public async Task AddPayment_ShouldAddPaymentToDatabase()
    {
        var context = GetInMemoryDbContext();
        var repository = new PaymentRepository(context);

        var payment = new Payment { PaymentID = 1, Amount = 50.00m, DeliveryCount = 10, TotalDeliveryPrice = 200.00m, DeliveryDriverID = 201, PaycheckDate = DateTime.Now, Status = "Pending" };

        await repository.AddPayment(payment);
        var result = await context.Payment.FirstOrDefaultAsync(p => p.PaymentID == 1);

        Assert.NotNull(result);
        Assert.Equal(50.00m, result?.Amount);
        Assert.Equal("Pending", result?.Status);
    }

    [Fact]
    public async Task UpdatePayment_ShouldUpdatePaymentInDatabase()
    {
        var context = GetInMemoryDbContext();
        var repository = new PaymentRepository(context);

        var payment = new Payment { PaymentID = 1, Amount = 50.00m, DeliveryCount = 10, TotalDeliveryPrice = 200.00m, DeliveryDriverID = 201, PaycheckDate = DateTime.Now, Status = "Pending" };
        await context.Payment.AddAsync(payment);
        await context.SaveChangesAsync();

        payment.Amount = 55.00m;
        payment.Status = "Paid";

        await repository.UpdatePayment(payment);
        var result = await context.Payment.FirstOrDefaultAsync(p => p.PaymentID == 1);

        Assert.NotNull(result);
        Assert.Equal(55.00m, result?.Amount);
        Assert.Equal("Paid", result?.Status);
    }
}
