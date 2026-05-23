using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ExploreGambia.API.Data.DesignTimeFactories
{
    public class ExploreGambiaAuthDbContextFactory : IDesignTimeDbContextFactory<ExploreGambiaAuthDbContext>
    {
        public ExploreGambiaAuthDbContext CreateDbContext(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<ExploreGambiaAuthDbContext>();
            optionsBuilder.UseNpgsql(config.GetConnectionString("DefaultConnection"),
                npsql => npsql.MigrationsHistoryTable("__EFMigrationsHistory_Auth"));

            return new ExploreGambiaAuthDbContext(optionsBuilder.Options);
        }
    }
}
