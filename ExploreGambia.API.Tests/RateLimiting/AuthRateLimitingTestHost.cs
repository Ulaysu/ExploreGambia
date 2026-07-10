using System.Globalization;
using System.Net;
using System.Security.Claims;
using System.Threading.RateLimiting;
using Asp.Versioning;
using ExploreGambia.API.Controllers;
using ExploreGambia.API.Models.Configurations;
using ExploreGambia.API.Models.Domain;
using ExploreGambia.API.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace ExploreGambia.API.Tests.RateLimiting
{
    internal sealed class AuthRateLimitingTestHost
    {
        private readonly TestServer server;

        public AuthRateLimitingTestHost(
            RecordingAuthService? authService = null,
            RateLimitingOptions? rateLimitingOptions = null)
        {
            AuthService = authService ?? new RecordingAuthService();
            RateLimitingOptions = rateLimitingOptions ?? new RateLimitingOptions();

            server = new TestServer(new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services
                        .AddControllers()
                        .AddApplicationPart(typeof(AuthController).Assembly);

                    services.AddApiVersioning(options =>
                    {
                        options.DefaultApiVersion = new ApiVersion(1, 0);
                        options.AssumeDefaultVersionWhenUnspecified = true;
                        options.ReportApiVersions = true;
                        options.ApiVersionReader = ApiVersionReader.Combine(
                            new UrlSegmentApiVersionReader(),
                            new HeaderApiVersionReader("x-api-version"),
                            new QueryStringApiVersionReader("api-version"));
                    });

                    AddRateLimiter(services, RateLimitingOptions);

                    services.AddSingleton<IAuthService>(AuthService);
                    services.AddSingleton(CreateUserManagerMock().Object);
                })
                .Configure(app =>
                {
                    app.UseRouting();
                    app.Use(async (context, next) =>
                    {
                        if (context.Request.Headers.TryGetValue("X-Test-Remote-Ip", out var headerValue) &&
                            IPAddress.TryParse(headerValue.ToString(), out var remoteIpAddress))
                        {
                            context.Connection.RemoteIpAddress = remoteIpAddress;
                        }

                        await next();
                    });
                    app.UseRateLimiter();
                    app.UseEndpoints(endpoints => endpoints.MapControllers());
                }));
        }

        public RecordingAuthService AuthService { get; }

        public RateLimitingOptions RateLimitingOptions { get; }

        public HttpClient CreateClient()
        {
            return server.CreateClient();
        }

        private static void AddRateLimiter(
            IServiceCollection services,
            RateLimitingOptions rateLimitingOptions)
        {
            var defaultRateLimitingOptions = new RateLimitingOptions();
            var loginRateLimit = ResolveRateLimitRule(
                rateLimitingOptions.Login,
                defaultRateLimitingOptions.Login);
            var registrationRateLimit = ResolveRateLimitRule(
                rateLimitingOptions.Registration,
                defaultRateLimitingOptions.Registration);
            var refreshTokenRateLimit = ResolveRateLimitRule(
                rateLimitingOptions.RefreshToken,
                defaultRateLimitingOptions.RefreshToken);

            services.AddRateLimiter(options =>
            {
                options.OnRejected = async (context, cancellationToken) =>
                {
                    if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                    {
                        context.HttpContext.Response.Headers.RetryAfter =
                            ((int)Math.Ceiling(retryAfter.TotalSeconds))
                            .ToString(CultureInfo.InvariantCulture);
                    }

                    context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    context.HttpContext.Response.ContentType = "application/json";

                    await context.HttpContext.Response.WriteAsJsonAsync(
                        new { message = "Too many requests. Please try again later." },
                        cancellationToken);
                };

                options.AddPolicy(AuthRateLimitPolicyNames.Login, httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        ResolveClientRateLimitPartitionKey(httpContext),
                        _ => CreateFixedWindowLimiterOptions(loginRateLimit)));

                options.AddPolicy(AuthRateLimitPolicyNames.Register, httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        ResolveClientRateLimitPartitionKey(httpContext),
                        _ => CreateFixedWindowLimiterOptions(registrationRateLimit)));

                options.AddPolicy(AuthRateLimitPolicyNames.RefreshToken, httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        ResolveClientRateLimitPartitionKey(httpContext),
                        _ => CreateFixedWindowLimiterOptions(refreshTokenRateLimit)));
            });
        }

        private static RateLimitRuleOptions ResolveRateLimitRule(
            RateLimitRuleOptions? configuredRule,
            RateLimitRuleOptions defaultRule)
        {
            if (configuredRule == null)
            {
                return defaultRule;
            }

            return new RateLimitRuleOptions
            {
                PermitLimit = configuredRule.PermitLimit > 0
                    ? configuredRule.PermitLimit
                    : defaultRule.PermitLimit,
                WindowSeconds = configuredRule.WindowSeconds > 0
                    ? configuredRule.WindowSeconds
                    : defaultRule.WindowSeconds
            };
        }

        private static string ResolveClientRateLimitPartitionKey(HttpContext httpContext)
        {
            var remoteIpAddress = httpContext.Connection.RemoteIpAddress?.ToString();

            return string.IsNullOrWhiteSpace(remoteIpAddress)
                ? "unknown-client"
                : remoteIpAddress;
        }

        private static FixedWindowRateLimiterOptions CreateFixedWindowLimiterOptions(
            RateLimitRuleOptions rule)
        {
            return new FixedWindowRateLimiterOptions
            {
                PermitLimit = rule.PermitLimit,
                Window = TimeSpan.FromSeconds(rule.WindowSeconds),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0,
                AutoReplenishment = true
            };
        }

        private static Mock<UserManager<ApplicationUser>> CreateUserManagerMock()
        {
            var userStore = new Mock<IUserStore<ApplicationUser>>();
            var passwordHasher = new Mock<IPasswordHasher<ApplicationUser>>();
            var lookupNormalizer = new Mock<ILookupNormalizer>();
            var serviceProvider = new Mock<IServiceProvider>();
            var logger = new Mock<ILogger<UserManager<ApplicationUser>>>();

            return new Mock<UserManager<ApplicationUser>>(
                userStore.Object,
                Options.Create(new IdentityOptions()),
                passwordHasher.Object,
                new List<IUserValidator<ApplicationUser>>(),
                new List<IPasswordValidator<ApplicationUser>>(),
                lookupNormalizer.Object,
                new IdentityErrorDescriber(),
                serviceProvider.Object,
                logger.Object);
        }
    }
}
