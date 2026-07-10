using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ExploreGambia.API.Models.DTOs;

namespace ExploreGambia.API.Tests.RateLimiting
{
    public class AuthRateLimitingTestHostTests
    {
        [Fact]
        public async Task LoginEndpoint_CanBeRequestedThroughTestHost()
        {
            var host = new AuthRateLimitingTestHost();
            var client = host.CreateClient();

            var response = await SendLoginRequestAsync(client);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(1, host.AuthService.LoginCalls);
        }

        [Fact]
        public async Task LoginRequests_BelowLimit_ReachAuthenticationLogic()
        {
            var host = new AuthRateLimitingTestHost();
            var client = host.CreateClient();

            for (var i = 0; i < 4; i++)
            {
                var response = await SendLoginRequestAsync(client);

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            }

            Assert.Equal(4, host.AuthService.LoginCalls);
        }

        [Fact]
        public async Task LoginRequests_ExceedLimit_ReturnTooManyRequests()
        {
            var host = new AuthRateLimitingTestHost();
            var client = host.CreateClient();

            for (var i = 0; i < 5; i++)
            {
                var response = await SendLoginRequestAsync(client);

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            }

            var rejectedResponse = await SendLoginRequestAsync(client);

            await AssertTooManyRequestsAsync(rejectedResponse);
            Assert.Equal(5, host.AuthService.LoginCalls);
        }

        [Fact]
        public async Task RegistrationRequests_ExceedLimit_ReturnTooManyRequests()
        {
            var host = new AuthRateLimitingTestHost();
            var client = host.CreateClient();

            for (var i = 0; i < 3; i++)
            {
                var response = await SendRegisterRequestAsync(client);

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            }

            var rejectedResponse = await SendRegisterRequestAsync(client);

            await AssertTooManyRequestsAsync(rejectedResponse);
            Assert.Equal(3, host.AuthService.RegisterCalls);
        }

        private static Task<HttpResponseMessage> SendLoginRequestAsync(HttpClient client)
        {
            return client.PostAsJsonAsync(
                "/api/v1/auth/login",
                new LoginRequestDto
                {
                    Email = "user@example.com",
                    Password = "password"
                });
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

        private static async Task AssertTooManyRequestsAsync(HttpResponseMessage response)
        {
            Assert.Equal(HttpStatusCode.TooManyRequests, response.StatusCode);
            Assert.NotNull(response.Headers.RetryAfter);

            var responseJson = await response.Content.ReadAsStringAsync();
            using var document = JsonDocument.Parse(responseJson);

            Assert.Equal(
                "Too many requests. Please try again later.",
                document.RootElement.GetProperty("message").GetString());
        }
    }
}
