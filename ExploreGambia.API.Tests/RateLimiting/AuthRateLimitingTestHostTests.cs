using System.Net;
using System.Net.Http.Json;
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

            var response = await client.PostAsJsonAsync(
                "/api/v1/auth/login",
                new LoginRequestDto
                {
                    Email = "user@example.com",
                    Password = "password"
                });

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(1, host.AuthService.LoginCalls);
        }
    }
}
