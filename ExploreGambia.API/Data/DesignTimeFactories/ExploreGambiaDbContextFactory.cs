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
            optionsBuilder.UseSqlServer(config.GetConnectionString("DefaultConnection"));

            return new ExploreGambiaDbContext(optionsBuilder.Options);
        }
    }
}
