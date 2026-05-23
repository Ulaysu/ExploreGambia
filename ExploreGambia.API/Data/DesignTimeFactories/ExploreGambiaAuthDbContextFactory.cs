using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ExploreGambia.API.Data.DesignTimeFactories
{
    public class ExploreGambiaAuthDbContextFactory : IDesignTimeDbContextFactory<ExploreGambiaAuthDbContext>
    {
        public ExploreGambiaAuthDbContext CreateDbContext(string[] args)
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
                ?? Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
                ?? "Development";

            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{environment}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<ExploreGambiaAuthDbContext>();
            optionsBuilder.UseNpgsql(config.GetConnectionString("DefaultConnection"),
                npsql => npsql.MigrationsHistoryTable("__EFMigrationsHistory_Auth"));

            return new ExploreGambiaAuthDbContext(optionsBuilder.Options);
        }
    }
}
