using FinanceTracker.Infrastructure.Data;
using FinanceTracker.Infrastructure.Migrations;
using Microsoft.EntityFrameworkCore;
using static FinanceTracker.Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

ValidateConfiguration(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "Finance Tracker API",
        Version = "v1",
        Description = "API para gerenciamento de transações financeiras"
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowCredentials()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

if (app.Environment.IsDevelopment())
{
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();



