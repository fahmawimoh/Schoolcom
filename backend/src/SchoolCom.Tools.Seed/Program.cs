using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SchoolCom.Infrastructure;
using SchoolCom.Infrastructure.Persistence;

var envPath = Path.Combine(Directory.GetCurrentDirectory(), ".env");
if (!File.Exists(envPath))
    envPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", ".env");
if (File.Exists(envPath))
    Env.Load(envPath);

var services = new ServiceCollection();
var config = new ConfigurationBuilder()
    .AddEnvironmentVariables()
    .Build();

services.AddInfrastructure(config);
var provider = services.BuildServiceProvider();

await using var scope = provider.CreateAsyncScope();
var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
await db.Database.MigrateAsync();
await DbSeeder.SeedAsync(db, scope.ServiceProvider);

Console.WriteLine("Database migrated and seeded successfully.");
