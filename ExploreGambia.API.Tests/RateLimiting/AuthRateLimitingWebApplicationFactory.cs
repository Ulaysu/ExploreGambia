using ExploreGambia.API.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ExploreGambia.API.Tests.RateLimiting
{
    internal sealed class AuthRateLimitingWebApplicationFactory : WebApplicationFactory<Program>
    {
        public AuthRateLimitingWebApplicationFactory()
        {
            // Program.cs requires these secrets during startup. Test values are
            // enough because the tests replace auth behavior before requests run.
            Environment.SetEnvironmentVariable("JWT_SECRET", "test-secret-key-with-enough-length");
            Environment.SetEnvironmentVariable("STRIPE_SECRET_KEY", "sk_test_rate_limiting");
        }

        public RecordingAuthService AuthService { get; } = new();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            // Development mode allows the test database connection to point at
            // localhost while still booting the real application pipeline.
            builder.UseEnvironment("Development");

            builder.ConfigureAppConfiguration(configurationBuilder =>
            {
                configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=exploregambia_tests;Username=test;Password=test",
                    // The tests assert middleware behavior, not database seeding.
                    ["DataSeeding:Enabled"] = "false",
                    // TestServer sends requests from loopback, so loopback must
                    // be trusted before X-Forwarded-For is applied.
                    ["ForwardedHeaders:KnownProxies:0"] = "127.0.0.1",
                    ["ForwardedHeaders:KnownProxies:1"] = "::1"
                });
            });

            builder.ConfigureServices(services =>
            {
                // Keep controller routing, filters, middleware, and rate-limit
                // policies real while replacing only the external auth work.
                services.RemoveAll<IAuthService>();
                services.AddSingleton<IAuthService>(AuthService);
            });
        }
    }
}
