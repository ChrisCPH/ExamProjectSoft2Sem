using Microsoft.EntityFrameworkCore;
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
app.MapControllers();

app.Run();