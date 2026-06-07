using Microsoft.Extensions.Configuration;
using Npgsql;

namespace SchoolCom.Infrastructure.Persistence;

public static class PostgresConnectionBuilder
{
    public static string Build(IConfiguration configuration)
    {
        var configuredConnection = configuration.GetConnectionString("DefaultConnection");
        if (!string.IsNullOrWhiteSpace(configuredConnection))
            return configuredConnection;

        var password = configuration["DB_PASSWORD"];
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException(
                "Database password is not configured. Set ConnectionStrings__DefaultConnection or DB_PASSWORD.");
        }

        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = configuration["DB_HOST"] ?? "localhost",
            Port = ParseInt(configuration["DB_PORT"], 5400),
            Database = configuration["DB_NAME"] ?? "schoolcom",
            Username = configuration["DB_USER"] ?? "postgres",
            Password = password,
            Pooling = true,
            MinPoolSize = ParseInt(configuration["DB_MIN_POOL_SIZE"], 0),
            MaxPoolSize = ParseInt(configuration["DB_MAX_POOL_SIZE"], 100),
            Timeout = ParseInt(configuration["DB_CONNECTION_TIMEOUT"], 15),
            CommandTimeout = ParseInt(configuration["DB_COMMAND_TIMEOUT"], 30),
            IncludeErrorDetail = false
        };

        var sslMode = configuration["DB_SSLMODE"];
        if (!string.IsNullOrWhiteSpace(sslMode) &&
            Enum.TryParse<SslMode>(sslMode, true, out var parsedSslMode))
        {
            builder.SslMode = parsedSslMode;
        }

        return builder.ConnectionString;
    }

    public static string DescribeForLogging(IConfiguration configuration)
    {
        var host = configuration["DB_HOST"] ?? "localhost";
        var port = configuration["DB_PORT"] ?? "5400";
        var database = configuration["DB_NAME"] ?? "schoolcom";
        var username = configuration["DB_USER"] ?? "postgres";
        return $"Host={host};Port={port};Database={database};Username={username};Password=***";
    }

    private static int ParseInt(string? value, int defaultValue) =>
        int.TryParse(value, out var parsed) ? parsed : defaultValue;
}
