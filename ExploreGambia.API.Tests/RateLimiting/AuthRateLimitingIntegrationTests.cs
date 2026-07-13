using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ExploreGambia.API.Controllers;
using ExploreGambia.API.Models.DTOs;
using Microsoft.AspNetCore.RateLimiting;

namespace ExploreGambia.API.Tests.RateLimiting
{
    [Collection(IntegrationTestCollection.Name)]
    public class AuthRateLimitingIntegrationTests
    {
        [Fact]
        public async Task LoginEndpoint_CanBeRequestedThroughRealApplicationPipeline()
        {
            using var factory = new AuthRateLimitingWebApplicationFactory();
            var client = factory.CreateClient();

            var response = await SendLoginRequestAsync(client);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(1, factory.AuthService.LoginCalls);
        }

        [Fact]
        public async Task LoginRequests_BelowLimit_ReachAuthenticationLogic()
        {
            using var factory = new AuthRateLimitingWebApplicationFactory();
            var client = factory.CreateClient();

            for (var i = 0; i < 4; i++)
            {
                var response = await SendLoginRequestAsync(client);

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            }

            Assert.Equal(4, factory.AuthService.LoginCalls);
        }

        [Fact]
        public async Task LoginRequests_ExceedLimit_ReturnTooManyRequests()
        {
            using var factory = new AuthRateLimitingWebApplicationFactory();
            var client = factory.CreateClient();

            for (var i = 0; i < 5; i++)
            {
                var response = await SendLoginRequestAsync(client);

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            }

            var rejectedResponse = await SendLoginRequestAsync(client);

            await AssertTooManyRequestsAsync(rejectedResponse);
            Assert.Equal(5, factory.AuthService.LoginCalls);
        }

        [Fact]
        public async Task RegistrationRequests_ExceedLimit_ReturnTooManyRequests()
        {
            using var factory = new AuthRateLimitingWebApplicationFactory();
            var client = factory.CreateClient();

            for (var i = 0; i < 3; i++)
            {
                var response = await SendRegisterRequestAsync(client);

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            }

            var rejectedResponse = await SendRegisterRequestAsync(client);

            await AssertTooManyRequestsAsync(rejectedResponse);
            Assert.Equal(3, factory.AuthService.RegisterCalls);
        }

        [Fact]
        public async Task RefreshTokenRequests_ExceedLimit_ReturnTooManyRequests()
        {
            using var factory = new AuthRateLimitingWebApplicationFactory();
            var client = factory.CreateClient();

            for (var i = 0; i < 10; i++)
            {
                var response = await SendRefreshTokenRequestAsync(client);

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            }

            var rejectedResponse = await SendRefreshTokenRequestAsync(client);

            await AssertTooManyRequestsAsync(rejectedResponse);
            Assert.Equal(10, factory.AuthService.RefreshTokenCalls);
        }

        [Fact]
        public async Task LoginRateLimit_IsIsolatedByClientIp()
        {
            using var factory = new AuthRateLimitingWebApplicationFactory();
            var client = factory.CreateClient();

            // Exhaust the login budget for one forwarded client IP.
            for (var i = 0; i < 5; i++)
            {
                var response = await SendLoginRequestAsync(client, "10.0.0.1");

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            }

            var rejectedResponse = await SendLoginRequestAsync(client, "10.0.0.1");
            await AssertTooManyRequestsAsync(rejectedResponse);

            // A different forwarded client IP should still have a fresh bucket,
            // proving the limiter partitions on the client address after
            // ForwardedHeadersMiddleware has processed the request.
            var isolatedClientResponse = await SendLoginRequestAsync(client, "10.0.0.2");

            Assert.Equal(HttpStatusCode.OK, isolatedClientResponse.StatusCode);
            Assert.Equal(6, factory.AuthService.LoginCalls);
        }

        [Fact]
        public void UnaffectedAuthEndpoints_DoNotHaveRateLimitAttributes()
        {
            var methodNames = new[]
            {
                nameof(AuthController.LogoutAsync),
                nameof(AuthController.GetCurrentUserAsync),
                nameof(AuthController.UpdateCurrentUserAsync)
            };

            foreach (var methodName in methodNames)
            {
                var method = typeof(AuthController).GetMethod(methodName);

                Assert.NotNull(method);
                Assert.Empty(method.GetCustomAttributes(typeof(EnableRateLimitingAttribute), inherit: false));
            }
        }

        private static Task<HttpResponseMessage> SendLoginRequestAsync(
            HttpClient client,
            string? remoteIpAddress = null)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/auth/login")
            {
                Content = JsonContent.Create(new LoginRequestDto
                {
                    Email = "user@example.com",
                    Password = "password"
                })
            };

            if (!string.IsNullOrWhiteSpace(remoteIpAddress))
            {
                // Use production proxy headers instead of a test-only shortcut
                // so the real forwarded-header middleware is part of the proof.
                request.Headers.Add("X-Forwarded-For", remoteIpAddress);
                request.Headers.Add("X-Forwarded-Proto", "https");
            }

            return client.SendAsync(request);
        }

        private static Task<HttpResponseMessage> SendRegisterRequestAsync(HttpClient client)
        {
            return client.PostAsJsonAsync(
                "/api/v1/auth/register",
                new RegisterRequestDto
                {
                    Email = "new-user@example.com",
                    Password = "password",
                    FirstName = "New",
                    LastName = "User",
                    Roles = new[] { "User" }
                });
        }

        private static Task<HttpResponseMessage> SendRefreshTokenRequestAsync(HttpClient client)
        {
            return client.PostAsJsonAsync(
                "/api/v1/auth/refresh-token",
                new RefreshTokenRequestDto
                {
                    AccessToken = "expired-access-token",
                    RefreshToken = "active-refresh-token"
                });
        }

        private static async Task AssertTooManyRequestsAsync(HttpResponseMessage response)
        {
            Assert.Equal(HttpStatusCode.TooManyRequests, response.StatusCode);
            Assert.NotNull(response.Headers.RetryAfter);

            // Keep the response contract pinned so future limiter changes do
            // not accidentally break frontend/client error handling.
            var responseJson = await response.Content.ReadAsStringAsync();
            using var document = JsonDocument.Parse(responseJson);

            Assert.Equal(
                "Too many requests. Please try again later.",
                document.RootElement.GetProperty("message").GetString());
        }
    }
}
