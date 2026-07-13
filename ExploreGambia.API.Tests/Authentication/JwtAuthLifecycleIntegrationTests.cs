using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using ExploreGambia.API.Models.DTOs;

namespace ExploreGambia.API.Tests.Authentication
{
    [Collection(IntegrationTestCollection.Name)]
    public class JwtAuthLifecycleIntegrationTests
    {
        private const string Password = "Password123!";

        [Fact]
        public async Task Login_WithValidCredentials_ReturnsJwtRefreshTokenAndExpectedClaims()
        {
            using var factory = new JwtAuthLifecycleWebApplicationFactory();
            var client = factory.CreateClient();
            var user = await factory.CreateUserAsync(
                role: "User",
                email: "jwt-user@example.com",
                password: Password,
                firstName: "Jwt",
                lastName: "User");

            var login = await factory.LoginAsync(client, user.Email!, Password);

            Assert.False(string.IsNullOrWhiteSpace(login.JwtToken));
            Assert.False(string.IsNullOrWhiteSpace(login.RefreshToken));
            Assert.Null(login.Error);

            var jwt = JwtAuthLifecycleWebApplicationFactory.ReadJwt(login.JwtToken);

            Assert.Equal(JwtAuthLifecycleWebApplicationFactory.JwtIssuer, jwt.Issuer);
            Assert.Contains(JwtAuthLifecycleWebApplicationFactory.JwtAudience, jwt.Audiences);
            Assert.True(jwt.ValidTo > DateTime.UtcNow);
            Assert.NotNull(jwt.Claims.SingleOrDefault(claim => claim.Type == JwtRegisteredClaimNames.Exp));
            Assert.Equal(user.Id, FindClaimValue(jwt, ClaimTypes.NameIdentifier, "nameid"));
            Assert.Equal(user.Email, FindClaimValue(jwt, ClaimTypes.Email, JwtRegisteredClaimNames.Email, "email"));
            Assert.Equal("User", FindClaimValue(jwt, ClaimTypes.Role, "role"));

            var storedUser = await factory.FindUserByEmailAsync(user.Email!);

            Assert.Equal(login.RefreshToken, storedUser.RefreshToken);
            Assert.True(storedUser.RefreshTokenExpiryTime > DateTime.UtcNow.AddDays(29));
        }

        [Fact]
        public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
        {
            using var factory = new JwtAuthLifecycleWebApplicationFactory();
            var client = factory.CreateClient();
            var user = await factory.CreateUserAsync(
                role: "User",
                email: "invalid-login@example.com",
                password: Password);

            var response = await client.PostAsJsonAsync(
                "/api/v1/auth/login",
                new LoginRequestDto
                {
                    Email = user.Email!,
                    Password = "wrong-password"
                });

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        private static string? FindClaimValue(JwtSecurityToken jwt, params string[] claimTypes)
        {
            return jwt.Claims.FirstOrDefault(claim => claimTypes.Contains(claim.Type))?.Value;
        }
    }
}
