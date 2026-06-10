using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace SchoolCom.Infrastructure.Persistence;

public static class DatabaseCleaner
{
    public static async Task ClearAllDataAsync(
        ApplicationDbContext db,
        ILogger? logger = null,
        CancellationToken cancellationToken = default)
    {
        var tableNames = db.Model.GetEntityTypes()
            .Select(entityType =>
            {
                var table = entityType.GetTableName();
                if (string.IsNullOrWhiteSpace(table)) return null;

                var schema = entityType.GetSchema() ?? "public";
                return QuoteIdentifier(schema) + "." + QuoteIdentifier(table);
            })
            .Where(name => name is not null)
            .Cast<string>()
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (tableNames.Count == 0)
        {
            logger?.LogWarning("No application tables found to truncate.");
            return;
        }

        logger?.LogInformation(
            "Truncating {Count} table(s). Schema and migration history are preserved.",
            tableNames.Count);

        var truncateSql =
            $"TRUNCATE TABLE {string.Join(", ", tableNames)} RESTART IDENTITY CASCADE;";

        await db.Database.ExecuteSqlRawAsync(truncateSql, cancellationToken);

        logger?.LogInformation("All application data cleared successfully.");
    }

    private static string QuoteIdentifier(string identifier) =>
        "\"" + identifier.Replace("\"", "\"\"") + "\"";
}
