using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace SchoolCom.Infrastructure.Persistence;

public static class DatabaseInitializer
{
    public static async Task InitializeAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        logger.LogInformation(
            "Connecting to PostgreSQL ({Connection})",
            PostgresConnectionBuilder.DescribeForLogging(configuration));

        if (!await db.Database.CanConnectAsync(cancellationToken))
        {
            throw new InvalidOperationException(
                "Cannot connect to PostgreSQL. Verify the server is running, credentials in backend/.env are correct, and the database exists.");
        }

        var pending = await db.Database.GetPendingMigrationsAsync(cancellationToken);
        var pendingList = pending.ToList();
        if (pendingList.Count > 0)
        {
            logger.LogInformation("Applying {Count} pending migration(s): {Migrations}",
                pendingList.Count, string.Join(", ", pendingList));
        }

        await db.Database.MigrateAsync(cancellationToken);
        await DbSeeder.SeedAsync(db, scope.ServiceProvider);

        var applied = await db.Database.GetAppliedMigrationsAsync(cancellationToken);
        logger.LogInformation(
            "PostgreSQL ready. Database={Database}, AppliedMigrations={MigrationCount}",
            db.Database.GetDbConnection().Database,
            applied.Count());
    }
}
