using ManagementService.Models;
using ManagementService.Data;
using ManagementService.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

public class FeeRepositoryTests
{
    private ManagementDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ManagementDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new ManagementDbContext(options);
    }

    [Fact]
    public async Task GetAllFees_ShouldReturnAllFees()
    {
        var context = GetInMemoryDbContext();
        var repository = new FeeRepository(context);

        var fees = new List<Fee>
        {
            new Fee { FeeID = 1, Amount = 10.50m, OrderCount = 2, TotalOrderPrice = 20.00m, RestaurantID = 101, InvoiceDate = DateTime.Now, DueDate = DateTime.Now.AddDays(5), Status = "Pending" },
            new Fee { FeeID = 2, Amount = 15.00m, OrderCount = 3, TotalOrderPrice = 45.00m, RestaurantID = 102, InvoiceDate = DateTime.Now, DueDate = DateTime.Now.AddDays(5), Status = "Paid" }
        };

        await context.Fee.AddRangeAsync(fees);
        await context.SaveChangesAsync();

        var result = await repository.GetAllFees();

        Assert.Equal(2, result.Count);
        Assert.Contains(result, f => f.FeeID == 1 && f.Amount == 10.50m);
        Assert.Contains(result, f => f.FeeID == 2 && f.Status == "Paid");
    }

    [Fact]
    public async Task GetFeeById_ShouldReturnCorrectFee()
    {
        var context = GetInMemoryDbContext();
        var repository = new FeeRepository(context);

        var fee = new Fee { FeeID = 1, Amount = 10.50m, OrderCount = 2, TotalOrderPrice = 20.00m, RestaurantID = 101, InvoiceDate = DateTime.Now, DueDate = DateTime.Now.AddDays(5), Status = "Pending" };
        await context.Fee.AddAsync(fee);
        await context.SaveChangesAsync();

        var result = await repository.GetFeeById(1);

        Assert.NotNull(result);
        Assert.Equal(10.50m, result?.Amount);
        Assert.Equal("Pending", result?.Status);
    }

    [Fact]
    public async Task AddFee_ShouldAddFeeToDatabase()
    {
        var context = GetInMemoryDbContext();
        var repository = new FeeRepository(context);

        var fee = new Fee { FeeID = 1, Amount = 10.50m, OrderCount = 2, TotalOrderPrice = 20.00m, RestaurantID = 101, InvoiceDate = DateTime.Now, DueDate = DateTime.Now.AddDays(5), Status = "Pending" };

        await repository.AddFee(fee);
        var result = await context.Fee.FirstOrDefaultAsync(f => f.FeeID == 1);

        Assert.NotNull(result);
        Assert.Equal(10.50m, result?.Amount);
        Assert.Equal("Pending", result?.Status);
    }

    [Fact]
    public async Task UpdateFee_ShouldUpdateFeeInDatabase()
    {
        var context = GetInMemoryDbContext();
        var repository = new FeeRepository(context);

        var fee = new Fee { FeeID = 1, Amount = 10.50m, OrderCount = 2, TotalOrderPrice = 20.00m, RestaurantID = 101, InvoiceDate = DateTime.Now, DueDate = DateTime.Now.AddDays(5), Status = "Pending" };
        await context.Fee.AddAsync(fee);
        await context.SaveChangesAsync();

        fee.Amount = 12.50m;
        fee.Status = "Paid";

        await repository.UpdateFee(fee);
        var result = await context.Fee.FirstOrDefaultAsync(f => f.FeeID == 1);

        Assert.NotNull(result);
        Assert.Equal(12.50m, result?.Amount);
        Assert.Equal("Paid", result?.Status);
    }
}
