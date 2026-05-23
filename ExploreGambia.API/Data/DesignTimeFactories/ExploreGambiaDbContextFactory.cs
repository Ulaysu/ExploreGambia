using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ExploreGambia.API.Data.DesignTimeFactories
{
    public class ExploreGambiaDbContextFactory : IDesignTimeDbContextFactory<ExploreGambiaDbContext>
    {
        public ExploreGambiaDbContext CreateDbContext(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<ExploreGambiaDbContext>();
            optionsBuilder.UseNpgsql(config.GetConnectionString("DefaultConnection"),
                 npsql => npsql.MigrationsHistoryTable("__EFMigrationsHistory_App"));

            return new ExploreGambiaDbContext(optionsBuilder.Options);
        }
    }
}
