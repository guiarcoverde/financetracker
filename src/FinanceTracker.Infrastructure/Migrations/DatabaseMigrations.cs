using FinanceTracker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FinanceTracker.Infrastructure.Migrations;

public static class DatabaseMigrations
{
    public static async Task MigrateDatabase(IServiceProvider serviceProvider)
    {
        var dbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();

        await dbContext.Database.MigrateAsync();
    }
}