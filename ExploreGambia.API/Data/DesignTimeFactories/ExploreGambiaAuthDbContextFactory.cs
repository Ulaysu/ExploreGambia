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
            optionsBuilder.UseSqlServer(config.GetConnectionString("IdentityConnection"));

            return new ExploreGambiaAuthDbContext(optionsBuilder.Options);
        }
    }
}
