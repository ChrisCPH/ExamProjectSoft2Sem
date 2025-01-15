using FeedbackService.Models;
using FeedbackService.Controllers;
using Microsoft.EntityFrameworkCore;
using Prometheus;
using FeedbackService.Services;
using FeedbackService.Data;
using FeedbackService.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<FeedbackDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IFeedbackRepository, FeedbackRepository>();
builder.Services.AddScoped<IFeedbackService, FeedbackService.Services.FeedbackService>();

builder.Services.AddHttpClient();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

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
