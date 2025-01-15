using Microsoft.EntityFrameworkCore;
using Prometheus;
using ManagementService.Data;
using ManagementService.Models;
using ManagementService.Repositories;
using ManagementService.Services;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ManagementDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped<IFeeService, FeeService>();
builder.Services.AddScoped<IFeeRepository, FeeRepository>();

builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();

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