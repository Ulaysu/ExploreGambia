using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using ExploreGambia.API.Data;
using ExploreGambia.API.Models.Domain;
using ExploreGambia.API.Models.DTOs;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;

namespace ExploreGambia.API.Tests.Authentication
{
    internal sealed class JwtAuthLifecycleWebApplicationFactory : WebApplicationFactory<Program>
    {
        public const string JwtSecret = "jwt-lifecycle-test-secret-key-with-enough-length";
        public const string JwtIssuer = "ExploreGambia.JwtLifecycle.Tests";
        public const string JwtAudience = "ExploreGambia.JwtLifecycle.Tests.Client";

        private readonly InMemoryDatabaseRoot databaseRoot = new();
        private readonly string appDatabaseName = $"jwt-auth-lifecycle-app-{Guid.NewGuid()}";
        private readonly string authDatabaseName = $"jwt-auth-lifecycle-auth-{Guid.NewGuid()}";
        private readonly ServiceProvider inMemoryServiceProvider =
            new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();
        private bool databaseInitialized;

        public JwtAuthLifecycleWebApplicationFactory()
        {
            Environment.SetEnvironmentVariable("JWT_SECRET", JwtSecret);
            Environment.SetEnvironmentVariable("STRIPE_SECRET_KEY", "sk_test_jwt_lifecycle");
        }

        public async Task<ApplicationUser> CreateUserAsync(
            string role,
            string email,
            string password = "Password123!",
            string firstName = "Lifecycle",
            string lastName = "User")
        {
            await InitializeDatabaseAsync();

            using var scope = Services.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                IsActive = true
            };

            var createResult = await userManager.CreateAsync(user, password);
            Assert.True(createResult.Succeeded, FormatIdentityErrors(createResult));

            var roleResult = await userManager.AddToRoleAsync(user, role);
            Assert.True(roleResult.Succeeded, FormatIdentityErrors(roleResult));

            return user;
        }

        public async Task<LoginResponseDto> LoginAsync(HttpClient client, string email, string password = "Password123!")
        {
            var response = await client.PostAsJsonAsync(
                "/api/v1/auth/login",
                new LoginRequestDto
                {
                    Email = email,
                    Password = password
                });

            response.EnsureSuccessStatusCode();

            return (await response.Content.ReadFromJsonAsync<LoginResponseDto>())!;
        }

        public async Task<ApplicationUser> FindUserByEmailAsync(string email)
        {
            await InitializeDatabaseAsync();

            using var scope = Services.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var user = await userManager.FindByEmailAsync(email);

            Assert.NotNull(user);

            return user;
        }

        public static JwtSecurityToken ReadJwt(string accessToken)
        {
            return new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
        }

        public static string CreateExpiredAccessToken(ApplicationUser user, string role)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim(ClaimTypes.Role, role)
            };

            var credentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtSecret)),
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: JwtIssuer,
                audience: JwtAudience,
                claims: claims,
                notBefore: DateTime.UtcNow.AddMinutes(-30),
                expires: DateTime.UtcNow.AddMinutes(-10),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public static HttpClient CreateBearerClient(string accessToken, HttpClient client)
        {
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken);

            return client;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");

            builder.ConfigureAppConfiguration(configurationBuilder =>
            {
                configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=exploregambia_jwt_lifecycle_tests;Username=test;Password=test",
                    ["DataSeeding:Enabled"] = "false",
                    ["Jwt:Issuer"] = JwtIssuer,
                    ["Jwt:Audience"] = JwtAudience,
                    ["ForwardedHeaders:KnownProxies:0"] = "127.0.0.1",
                    ["ForwardedHeaders:KnownProxies:1"] = "::1"
                });
            });

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<DbContextOptions<ExploreGambiaDbContext>>();
                services.RemoveAll<DbContextOptions<ExploreGambiaAuthDbContext>>();

                services.AddDbContext<ExploreGambiaDbContext>(options =>
                    options
                        .UseInMemoryDatabase(appDatabaseName, databaseRoot)
                        .UseInternalServiceProvider(inMemoryServiceProvider));

                services.AddDbContext<ExploreGambiaAuthDbContext>(options =>
                    options
                        .UseInMemoryDatabase(authDatabaseName, databaseRoot)
                        .UseInternalServiceProvider(inMemoryServiceProvider));
            });
        }

        private async Task InitializeDatabaseAsync()
        {
            if (databaseInitialized)
            {
                return;
            }

            using var scope = Services.CreateScope();
            var appDbContext = scope.ServiceProvider.GetRequiredService<ExploreGambiaDbContext>();
            var authDbContext = scope.ServiceProvider.GetRequiredService<ExploreGambiaAuthDbContext>();

            await appDbContext.Database.EnsureDeletedAsync();
            await authDbContext.Database.EnsureDeletedAsync();
            await appDbContext.Database.EnsureCreatedAsync();
            await authDbContext.Database.EnsureCreatedAsync();

            databaseInitialized = true;
        }

        private static string FormatIdentityErrors(IdentityResult result)
        {
            return string.Join(", ", result.Errors.Select(error => error.Description));
        }
    }
}
