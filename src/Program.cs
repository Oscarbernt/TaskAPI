using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;
using TaskHub.API.Data;
using TaskHub.API.Infrastructure.Logging;
using TaskHub.API.Repositories;
using TaskHub.API.Repositories.Interfaces;
using TaskHub.API.Services;
using TaskHub.API.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddLogging(loggingBuilder =>
{
    loggingBuilder.ClearProviders();
    loggingBuilder.AddSerilog(logger: new LoggerConfiguration()
        .MinimumLevel.Information()
        .MinimumLevel.Override("Microsoft.AspNetCore.Routing", LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)

        .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day, formatProvider: new CultureInfo("en-US"))
        .CreateLogger());
    loggingBuilder.SetMinimumLevel(LogLevel.Information);
});

// Configure Entity Framework Core with SQLite
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddScoped<ITaskService, TaskService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<RequestLoggingMiddleware>();
app.UseHttpsRedirection();
app.MapControllers();
app.Run();
