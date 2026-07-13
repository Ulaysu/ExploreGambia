using System.Net.Http.Headers;
using ExploreGambia.API.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ExploreGambia.API.Tests.Authorization
{
    internal sealed class AuthorizationTestWebApplicationFactory : WebApplicationFactory<Program>
    {
        public const string JwtSecret = "authorization-test-secret-key-with-enough-length";
        public const string JwtIssuer = "ExploreGambia.Authorization.Tests";
        public const string JwtAudience = "ExploreGambia.Authorization.Tests.Client";

        private readonly Action<IServiceCollection>? configureServices;

        public AuthorizationTestWebApplicationFactory(Action<IServiceCollection>? configureServices = null)
        {
            this.configureServices = configureServices;
            Environment.SetEnvironmentVariable("JWT_SECRET", JwtSecret);
            Environment.SetEnvironmentVariable("STRIPE_SECRET_KEY", "sk_test_authorization");
        }

        public RecordingLogoutAuthService AuthService { get; } = new();

        public HttpClient CreateAuthenticatedClient(string role, string? userId = null, string? email = null)
        {
            var client = CreateClient();
            var token = TestJwtTokenFactory.CreateToken(
                role,
                userId ?? TestUsers.UserIdFor(role),
                email ?? TestUsers.EmailFor(role));

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            return client;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");

            builder.ConfigureAppConfiguration(configurationBuilder =>
            {
                configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=exploregambia_authorization_tests;Username=test;Password=test",
                    ["DataSeeding:Enabled"] = "false",
                    ["Jwt:Issuer"] = JwtIssuer,
                    ["Jwt:Audience"] = JwtAudience,
                    ["ForwardedHeaders:KnownProxies:0"] = "127.0.0.1",
                    ["ForwardedHeaders:KnownProxies:1"] = "::1"
                });
            });

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IAuthService>();
                services.AddSingleton<IAuthService>(AuthService);
                configureServices?.Invoke(services);
            });
        }
    }
}
