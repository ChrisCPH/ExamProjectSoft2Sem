using Microsoft.EntityFrameworkCore;
using Prometheus;
using SearchService.Data;
using SearchService.Repositories;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<SearchDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped<IRestaurantRepository, RestaurantRepository>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

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