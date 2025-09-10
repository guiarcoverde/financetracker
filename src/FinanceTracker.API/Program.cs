using FinanceTracker.Infrastructure.Data;
using FinanceTracker.Infrastructure.Migrations;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DevConnection")));

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

if (app.Environment.IsDevelopment())
{
    await MigrateDatabase();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

async Task MigrateDatabase()
{
    await using var scope = app.Services.CreateAsyncScope();
    await DatabaseMigrations.MigrateDatabase(scope.ServiceProvider);
}

namespace FinanceTracker.API
{
    public partial class Program { }
}
