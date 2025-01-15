using Microsoft.EntityFrameworkCore;
using Prometheus;
using OrderService.Data;
using OrderService.Models;
using OrderService.Repositories;
using OrderService.Services;
using OrderService.Controllers;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped<IOrderService, OrderService.Services.OrderService>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();

builder.Services.AddHttpClient();

builder.Services.AddControllers();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthorization();

app.UseHttpMetrics();

app.MapControllers();

app.MapMetrics();

var requestCounter = Metrics.CreateCounter("http_requests_total", "Total HTTP requests received.");

app.Use(async (context, next) =>
{
    requestCounter.Inc();
    await next.Invoke();
});

app.Run();
