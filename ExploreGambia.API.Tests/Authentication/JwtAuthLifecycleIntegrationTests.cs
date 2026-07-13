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

        public static IEnumerable<object[]> Roles()
        {
            yield return new object[] { "User", "role-user@example.com" };
            yield return new object[] { "Guide", "role-guide@example.com" };
            yield return new object[] { "Admin", "role-admin@example.com" };
        }

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

        [Theory]
        [MemberData(nameof(Roles))]
        public async Task AuthMe_WithLoginIssuedJwt_ReturnsAuthenticatedUserIdentity(string role, string email)
        {
            using var factory = new JwtAuthLifecycleWebApplicationFactory();
            var client = factory.CreateClient();
            var user = await factory.CreateUserAsync(
                role: role,
                email: email,
                password: Password,
                firstName: role,
                lastName: "Lifecycle");
            var login = await factory.LoginAsync(client, user.Email!, Password);

            JwtAuthLifecycleWebApplicationFactory.CreateBearerClient(login.JwtToken, client);

            var response = await client.GetAsync("/api/v1/auth/me");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var profile = await response.Content.ReadFromJsonAsync<AuthMeResponseDto>();

            Assert.NotNull(profile);
            Assert.Equal(user.Id, profile.UserId);
            Assert.Equal(user.Email, profile.Email);
            Assert.Equal(role, profile.FirstName);
            Assert.Equal("Lifecycle", profile.LastName);
            Assert.True(profile.IsAuthenticated);
            Assert.Contains(role, profile.Roles);
            Assert.Equal(role, FindClaimValue(
                JwtAuthLifecycleWebApplicationFactory.ReadJwt(login.JwtToken),
                ClaimTypes.Role,
                "role"));
        }

        [Fact]
        public async Task RoleProtectedEndpoints_WithLoginIssuedJwt_ApplyRoleAuthorization()
        {
            using var factory = new JwtAuthLifecycleWebApplicationFactory();
            var adminClient = factory.CreateClient();
            var userClient = factory.CreateClient();
            var guideClient = factory.CreateClient();

            var admin = await factory.CreateUserAsync("Admin", "protected-admin@example.com", Password);
            var user = await factory.CreateUserAsync("User", "protected-user@example.com", Password);
            var guide = await factory.CreateUserAsync("Guide", "protected-guide@example.com", Password);

            var adminLogin = await factory.LoginAsync(adminClient, admin.Email!, Password);
            var userLogin = await factory.LoginAsync(userClient, user.Email!, Password);
            var guideLogin = await factory.LoginAsync(guideClient, guide.Email!, Password);

            JwtAuthLifecycleWebApplicationFactory.CreateBearerClient(adminLogin.JwtToken, adminClient);
            JwtAuthLifecycleWebApplicationFactory.CreateBearerClient(userLogin.JwtToken, userClient);
            JwtAuthLifecycleWebApplicationFactory.CreateBearerClient(guideLogin.JwtToken, guideClient);

            var adminDashboardResponse = await adminClient.GetAsync("/api/v1/admin/dashboard");
            var userDashboardResponse = await userClient.GetAsync("/api/v1/admin/dashboard");
            var userBookingsResponse = await userClient.GetAsync("/api/v1/bookings/my-bookings");
            var guideBookingsResponse = await guideClient.GetAsync("/api/v1/bookings/my-bookings");
            var guideProfileResponse = await guideClient.GetAsync("/api/v1/tour-guides/me");
            var userGuideProfileResponse = await userClient.GetAsync("/api/v1/tour-guides/me");

            Assert.Equal(HttpStatusCode.OK, adminDashboardResponse.StatusCode);
            Assert.Equal(HttpStatusCode.Forbidden, userDashboardResponse.StatusCode);
            Assert.Equal(HttpStatusCode.OK, userBookingsResponse.StatusCode);
            Assert.Equal(HttpStatusCode.Forbidden, guideBookingsResponse.StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, guideProfileResponse.StatusCode);
            Assert.Equal(HttpStatusCode.Forbidden, userGuideProfileResponse.StatusCode);
        }

        [Fact]
        public async Task RefreshToken_WithValidTokenPair_RotatesRefreshTokenAndRejectsPreviousRefreshToken()
        {
            using var factory = new JwtAuthLifecycleWebApplicationFactory();
            var client = factory.CreateClient();
            var user = await factory.CreateUserAsync(
                role: "User",
                email: "refresh-rotation@example.com",
                password: Password);
            var login = await factory.LoginAsync(client, user.Email!, Password);

            await Task.Delay(TimeSpan.FromSeconds(1.1));

            var refreshResponse = await client.PostAsJsonAsync(
                "/api/v1/auth/refresh-token",
                new RefreshTokenRequestDto
                {
                    AccessToken = login.JwtToken,
                    RefreshToken = login.RefreshToken
                });

            Assert.Equal(HttpStatusCode.OK, refreshResponse.StatusCode);

            var refreshedTokens = await refreshResponse.Content.ReadFromJsonAsync<RefreshTokenResponseDto>();

            Assert.NotNull(refreshedTokens);
            Assert.False(string.IsNullOrWhiteSpace(refreshedTokens.AccessToken));
            Assert.False(string.IsNullOrWhiteSpace(refreshedTokens.RefreshToken));
            Assert.NotEqual(login.JwtToken, refreshedTokens.AccessToken);
            Assert.NotEqual(login.RefreshToken, refreshedTokens.RefreshToken);

            var storedUser = await factory.FindUserByEmailAsync(user.Email!);

            Assert.Equal(refreshedTokens.RefreshToken, storedUser.RefreshToken);

            var oldRefreshTokenResponse = await client.PostAsJsonAsync(
                "/api/v1/auth/refresh-token",
                new RefreshTokenRequestDto
                {
                    AccessToken = login.JwtToken,
                    RefreshToken = login.RefreshToken
                });

            Assert.Equal(HttpStatusCode.Unauthorized, oldRefreshTokenResponse.StatusCode);

            var secondRefreshResponse = await client.PostAsJsonAsync(
                "/api/v1/auth/refresh-token",
                new RefreshTokenRequestDto
                {
                    AccessToken = refreshedTokens.AccessToken,
                    RefreshToken = refreshedTokens.RefreshToken
                });

            Assert.Equal(HttpStatusCode.OK, secondRefreshResponse.StatusCode);
        }

        [Fact]
        public async Task Logout_WithLoginIssuedJwt_InvalidatesStoredRefreshToken()
        {
            using var factory = new JwtAuthLifecycleWebApplicationFactory();
            var client = factory.CreateClient();
            var user = await factory.CreateUserAsync(
                role: "User",
                email: "logout-revocation@example.com",
                password: Password);
            var login = await factory.LoginAsync(client, user.Email!, Password);

            JwtAuthLifecycleWebApplicationFactory.CreateBearerClient(login.JwtToken, client);

            var logoutResponse = await client.PostAsync("/api/v1/auth/logout", content: null);

            Assert.Equal(HttpStatusCode.OK, logoutResponse.StatusCode);

            var storedUser = await factory.FindUserByEmailAsync(user.Email!);

            Assert.Null(storedUser.RefreshToken);
            Assert.Null(storedUser.RefreshTokenExpiryTime);

            var refreshAfterLogoutResponse = await client.PostAsJsonAsync(
                "/api/v1/auth/refresh-token",
                new RefreshTokenRequestDto
                {
                    AccessToken = login.JwtToken,
                    RefreshToken = login.RefreshToken
                });

            Assert.Equal(HttpStatusCode.Unauthorized, refreshAfterLogoutResponse.StatusCode);
        }

        [Theory]
        [InlineData("random-refresh-token")]
        public async Task RefreshToken_WithInvalidRefreshToken_ReturnsUnauthorized(string invalidRefreshToken)
        {
            using var factory = new JwtAuthLifecycleWebApplicationFactory();
            var client = factory.CreateClient();
            var user = await factory.CreateUserAsync(
                role: "User",
                email: $"invalid-refresh-{Guid.NewGuid():N}@example.com",
                password: Password);
            var login = await factory.LoginAsync(client, user.Email!, Password);

            var response = await client.PostAsJsonAsync(
                "/api/v1/auth/refresh-token",
                new RefreshTokenRequestDto
                {
                    AccessToken = login.JwtToken,
                    RefreshToken = invalidRefreshToken
                });

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task RefreshToken_WithMissingRefreshToken_ReturnsBadRequest()
        {
            using var factory = new JwtAuthLifecycleWebApplicationFactory();
            var client = factory.CreateClient();
            var user = await factory.CreateUserAsync(
                role: "User",
                email: "missing-refresh-token@example.com",
                password: Password);
            var login = await factory.LoginAsync(client, user.Email!, Password);

            var response = await client.PostAsJsonAsync(
                "/api/v1/auth/refresh-token",
                new
                {
                    AccessToken = login.JwtToken
                });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task RefreshToken_WithRefreshTokenFromAnotherUser_ReturnsUnauthorized()
        {
            using var factory = new JwtAuthLifecycleWebApplicationFactory();
            var client = factory.CreateClient();
            var firstUser = await factory.CreateUserAsync(
                role: "User",
                email: "mismatched-refresh-first@example.com",
                password: Password);
            var secondUser = await factory.CreateUserAsync(
                role: "User",
                email: "mismatched-refresh-second@example.com",
                password: Password);
            var firstLogin = await factory.LoginAsync(client, firstUser.Email!, Password);
            var secondLogin = await factory.LoginAsync(client, secondUser.Email!, Password);

            var response = await client.PostAsJsonAsync(
                "/api/v1/auth/refresh-token",
                new RefreshTokenRequestDto
                {
                    AccessToken = firstLogin.JwtToken,
                    RefreshToken = secondLogin.RefreshToken
                });

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task ProtectedEndpoint_WithExpiredAccessToken_ReturnsUnauthorized()
        {
            using var factory = new JwtAuthLifecycleWebApplicationFactory();
            var client = factory.CreateClient();
            var user = await factory.CreateUserAsync(
                role: "User",
                email: "expired-access-token@example.com",
                password: Password);
            var expiredToken = JwtAuthLifecycleWebApplicationFactory.CreateExpiredAccessToken(user, "User");

            JwtAuthLifecycleWebApplicationFactory.CreateBearerClient(expiredToken, client);

            var response = await client.GetAsync("/api/v1/auth/me");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        private static string? FindClaimValue(JwtSecurityToken jwt, params string[] claimTypes)
        {
            return jwt.Claims.FirstOrDefault(claim => claimTypes.Contains(claim.Type))?.Value;
        }
    }
}
