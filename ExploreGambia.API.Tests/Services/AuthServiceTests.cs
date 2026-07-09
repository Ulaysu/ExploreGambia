using System.Security.Claims;
using ExploreGambia.API.Exceptions;
using ExploreGambia.API.Models.Domain;
using ExploreGambia.API.Models.DTOs;
using ExploreGambia.API.Repositories;
using ExploreGambia.API.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace ExploreGambia.API.Tests.Services
{
    public class AuthServiceTests
    {
        [Fact]
        public async Task LogoutAsync_WhenUserExists_ClearsRefreshTokenAndExpiry()
        {
            var user = new ApplicationUser
            {
                Id = "user-1",
                RefreshToken = "active-refresh-token",
                RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(10)
            };

            var userManager = CreateUserManagerMock();
            userManager.Setup(manager => manager.FindByIdAsync(user.Id))
                .ReturnsAsync(user);
            userManager.Setup(manager => manager.UpdateAsync(user))
                .ReturnsAsync(IdentityResult.Success);

            var service = CreateAuthService(userManager);

            await service.LogoutAsync(user.Id);

            Assert.Null(user.RefreshToken);
            Assert.Null(user.RefreshTokenExpiryTime);
            userManager.Verify(manager => manager.UpdateAsync(user), Times.Once);
        }

        [Fact]
        public async Task RefreshTokenAsync_WhenStoredTokenWasCleared_ThrowsAuthenticationFailedException()
        {
            var user = new ApplicationUser
            {
                Email = "user@example.com",
                RefreshToken = null,
                RefreshTokenExpiryTime = null
            };

            var userManager = CreateUserManagerMock();
            userManager.Setup(manager => manager.FindByEmailAsync(user.Email))
                .ReturnsAsync(user);

            var tokenRepository = new Mock<ITokenRepository>();
            tokenRepository.Setup(repository => repository.GetPrincipalFromExpiredToken("old-access-token"))
                .Returns(CreatePrincipal(user.Email));

            var service = CreateAuthService(userManager, tokenRepository);

            // A logged-out user has no stored refresh token, so the old client token must not rotate.
            await Assert.ThrowsAsync<AuthenticationFailedException>(() =>
                service.RefreshTokenAsync(new RefreshTokenRequestDto
                {
                    AccessToken = "old-access-token",
                    RefreshToken = "old-refresh-token"
                }));

            tokenRepository.Verify(repository => repository.GenerateRefreshToken(), Times.Never);
            userManager.Verify(manager => manager.UpdateAsync(It.IsAny<ApplicationUser>()), Times.Never);
        }

        [Theory]
        [InlineData("", "refresh-token")]
        [InlineData("   ", "refresh-token")]
        [InlineData("access-token", "")]
        [InlineData("access-token", "   ")]
        public async Task RefreshTokenAsync_WhenRequestTokensAreBlank_ThrowsAuthenticationFailedException(
            string accessToken,
            string refreshToken)
        {
            var userManager = CreateUserManagerMock();
            var tokenRepository = new Mock<ITokenRepository>();
            var service = CreateAuthService(userManager, tokenRepository);

            await Assert.ThrowsAsync<AuthenticationFailedException>(() =>
                service.RefreshTokenAsync(new RefreshTokenRequestDto
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken
                }));

            tokenRepository.Verify(repository => repository.GetPrincipalFromExpiredToken(It.IsAny<string>()), Times.Never);
            userManager.Verify(manager => manager.UpdateAsync(It.IsAny<ApplicationUser>()), Times.Never);
        }

        [Fact]
        public async Task RefreshTokenAsync_WhenStoredRefreshTokenIsMissing_ThrowsAuthenticationFailedException()
        {
            var user = new ApplicationUser
            {
                Email = "user@example.com",
                RefreshToken = "stored-refresh-token",
                RefreshTokenExpiryTime = null
            };

            var userManager = CreateUserManagerMock();
            userManager.Setup(manager => manager.FindByEmailAsync(user.Email))
                .ReturnsAsync(user);

            var tokenRepository = new Mock<ITokenRepository>();
            tokenRepository.Setup(repository => repository.GetPrincipalFromExpiredToken("access-token"))
                .Returns(CreatePrincipal(user.Email));

            var service = CreateAuthService(userManager, tokenRepository);

            await Assert.ThrowsAsync<AuthenticationFailedException>(() =>
                service.RefreshTokenAsync(new RefreshTokenRequestDto
                {
                    AccessToken = "access-token",
                    RefreshToken = "stored-refresh-token"
                }));

            tokenRepository.Verify(repository => repository.GenerateRefreshToken(), Times.Never);
            userManager.Verify(manager => manager.UpdateAsync(It.IsAny<ApplicationUser>()), Times.Never);
        }

        [Fact]
        public async Task RefreshTokenAsync_WhenRefreshTokenDoesNotMatch_ThrowsAuthenticationFailedException()
        {
            var user = new ApplicationUser
            {
                Email = "user@example.com",
                RefreshToken = "stored-refresh-token",
                RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(10)
            };

            var userManager = CreateUserManagerMock();
            userManager.Setup(manager => manager.FindByEmailAsync(user.Email))
                .ReturnsAsync(user);

            var tokenRepository = new Mock<ITokenRepository>();
            tokenRepository.Setup(repository => repository.GetPrincipalFromExpiredToken("access-token"))
                .Returns(CreatePrincipal(user.Email));

            var service = CreateAuthService(userManager, tokenRepository);

            await Assert.ThrowsAsync<AuthenticationFailedException>(() =>
                service.RefreshTokenAsync(new RefreshTokenRequestDto
                {
                    AccessToken = "access-token",
                    RefreshToken = "different-refresh-token"
                }));

            tokenRepository.Verify(repository => repository.GenerateRefreshToken(), Times.Never);
            userManager.Verify(manager => manager.UpdateAsync(It.IsAny<ApplicationUser>()), Times.Never);
        }

        [Fact]
        public async Task RefreshTokenAsync_WhenRefreshTokenIsExpired_ThrowsAuthenticationFailedException()
        {
            var user = new ApplicationUser
            {
                Email = "user@example.com",
                RefreshToken = "stored-refresh-token",
                RefreshTokenExpiryTime = DateTime.UtcNow.AddMinutes(-1)
            };

            var userManager = CreateUserManagerMock();
            userManager.Setup(manager => manager.FindByEmailAsync(user.Email))
                .ReturnsAsync(user);

            var tokenRepository = new Mock<ITokenRepository>();
            tokenRepository.Setup(repository => repository.GetPrincipalFromExpiredToken("access-token"))
                .Returns(CreatePrincipal(user.Email));

            var service = CreateAuthService(userManager, tokenRepository);

            await Assert.ThrowsAsync<AuthenticationFailedException>(() =>
                service.RefreshTokenAsync(new RefreshTokenRequestDto
                {
                    AccessToken = "access-token",
                    RefreshToken = "stored-refresh-token"
                }));

            tokenRepository.Verify(repository => repository.GenerateRefreshToken(), Times.Never);
            userManager.Verify(manager => manager.UpdateAsync(It.IsAny<ApplicationUser>()), Times.Never);
        }

        [Fact]
        public async Task LoginAsync_WhenCredentialsAreValid_StoresGeneratedRefreshTokenAndExpiry()
        {
            var user = new ApplicationUser
            {
                Email = "user@example.com",
                UserName = "user@example.com",
                IsActive = true
            };

            var userManager = CreateUserManagerMock();
            userManager.Setup(manager => manager.FindByEmailAsync(user.Email))
                .ReturnsAsync(user);
            userManager.Setup(manager => manager.CheckPasswordAsync(user, "password"))
                .ReturnsAsync(true);
            userManager.Setup(manager => manager.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { "traveller" });
            userManager.Setup(manager => manager.UpdateAsync(user))
                .ReturnsAsync(IdentityResult.Success);

            var tokenRepository = new Mock<ITokenRepository>();
            tokenRepository.Setup(repository => repository.CreateJWTToken(user, It.IsAny<List<string>>()))
                .Returns("login-access-token");
            tokenRepository.Setup(repository => repository.GenerateRefreshToken())
                .Returns("login-refresh-token");

            var service = CreateAuthService(userManager, tokenRepository);

            var response = await service.LoginAsync(new LoginRequestDto
            {
                Email = user.Email,
                Password = "password"
            });

            Assert.Equal("login-access-token", response.JwtToken);
            Assert.Equal("login-refresh-token", response.RefreshToken);
            Assert.Equal("login-refresh-token", user.RefreshToken);
            Assert.True(user.RefreshTokenExpiryTime > DateTime.UtcNow.AddDays(29));
            userManager.Verify(manager => manager.UpdateAsync(user), Times.Once);
        }

        [Fact]
        public async Task RefreshTokenAsync_WhenRefreshTokenIsValid_RotatesRefreshTokenAndExpiry()
        {
            var user = new ApplicationUser
            {
                Email = "user@example.com",
                RefreshToken = "old-refresh-token",
                RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(10)
            };

            var userManager = CreateUserManagerMock();
            userManager.Setup(manager => manager.FindByEmailAsync(user.Email))
                .ReturnsAsync(user);
            userManager.Setup(manager => manager.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { "traveller" });
            userManager.Setup(manager => manager.UpdateAsync(user))
                .ReturnsAsync(IdentityResult.Success);

            var tokenRepository = new Mock<ITokenRepository>();
            tokenRepository.Setup(repository => repository.GetPrincipalFromExpiredToken("old-access-token"))
                .Returns(CreatePrincipal(user.Email));
            tokenRepository.Setup(repository => repository.CreateJWTToken(user, It.IsAny<List<string>>()))
                .Returns("new-access-token");
            tokenRepository.Setup(repository => repository.GenerateRefreshToken())
                .Returns("new-refresh-token");

            var service = CreateAuthService(userManager, tokenRepository);

            var response = await service.RefreshTokenAsync(new RefreshTokenRequestDto
            {
                AccessToken = "old-access-token",
                RefreshToken = "old-refresh-token"
            });

            Assert.Equal("new-access-token", response.AccessToken);
            Assert.Equal("new-refresh-token", response.RefreshToken);
            Assert.Equal("new-refresh-token", user.RefreshToken);
            Assert.True(user.RefreshTokenExpiryTime > DateTime.UtcNow.AddDays(29));
            userManager.Verify(manager => manager.UpdateAsync(user), Times.Once);
        }

        [Fact]
        public async Task RefreshTokenAsync_AfterRotation_RejectsPreviousRefreshToken()
        {
            var user = new ApplicationUser
            {
                Email = "user@example.com",
                RefreshToken = "old-refresh-token",
                RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(10)
            };

            var userManager = CreateUserManagerMock();
            userManager.Setup(manager => manager.FindByEmailAsync(user.Email))
                .ReturnsAsync(user);
            userManager.Setup(manager => manager.GetRolesAsync(user))
                .ReturnsAsync(new List<string>());
            userManager.Setup(manager => manager.UpdateAsync(user))
                .ReturnsAsync(IdentityResult.Success);

            var tokenRepository = new Mock<ITokenRepository>();
            tokenRepository.Setup(repository => repository.GetPrincipalFromExpiredToken(It.IsAny<string>()))
                .Returns(CreatePrincipal(user.Email));
            tokenRepository.Setup(repository => repository.CreateJWTToken(user, It.IsAny<List<string>>()))
                .Returns("new-access-token");
            tokenRepository.Setup(repository => repository.GenerateRefreshToken())
                .Returns("new-refresh-token");

            var service = CreateAuthService(userManager, tokenRepository);

            await service.RefreshTokenAsync(new RefreshTokenRequestDto
            {
                AccessToken = "old-access-token",
                RefreshToken = "old-refresh-token"
            });

            // Rotation makes the previously accepted refresh token stale immediately.
            await Assert.ThrowsAsync<AuthenticationFailedException>(() =>
                service.RefreshTokenAsync(new RefreshTokenRequestDto
                {
                    AccessToken = "old-access-token",
                    RefreshToken = "old-refresh-token"
                }));

            Assert.Equal("new-refresh-token", user.RefreshToken);
            tokenRepository.Verify(repository => repository.GenerateRefreshToken(), Times.Once);
            userManager.Verify(manager => manager.UpdateAsync(user), Times.Once);
        }

        [Fact]
        public async Task LoginAsync_AfterLogout_IssuesNewRefreshTokenThatCanBeRotated()
        {
            var user = new ApplicationUser
            {
                Email = "user@example.com",
                UserName = "user@example.com",
                IsActive = true,
                RefreshToken = null,
                RefreshTokenExpiryTime = null
            };

            var userManager = CreateUserManagerMock();
            userManager.Setup(manager => manager.FindByEmailAsync(user.Email))
                .ReturnsAsync(user);
            userManager.Setup(manager => manager.CheckPasswordAsync(user, "password"))
                .ReturnsAsync(true);
            userManager.Setup(manager => manager.GetRolesAsync(user))
                .ReturnsAsync(new List<string>());
            userManager.Setup(manager => manager.UpdateAsync(user))
                .ReturnsAsync(IdentityResult.Success);

            var tokenRepository = new Mock<ITokenRepository>();
            tokenRepository.SetupSequence(repository => repository.CreateJWTToken(user, It.IsAny<List<string>>()))
                .Returns("login-access-token")
                .Returns("rotated-access-token");
            tokenRepository.SetupSequence(repository => repository.GenerateRefreshToken())
                .Returns("login-refresh-token")
                .Returns("rotated-refresh-token");
            tokenRepository.Setup(repository => repository.GetPrincipalFromExpiredToken("login-access-token"))
                .Returns(CreatePrincipal(user.Email));

            var service = CreateAuthService(userManager, tokenRepository);

            var loginResponse = await service.LoginAsync(new LoginRequestDto
            {
                Email = user.Email,
                Password = "password"
            });

            var refreshResponse = await service.RefreshTokenAsync(new RefreshTokenRequestDto
            {
                AccessToken = loginResponse.JwtToken,
                RefreshToken = loginResponse.RefreshToken
            });

            Assert.Equal("login-refresh-token", loginResponse.RefreshToken);
            Assert.Equal("rotated-access-token", refreshResponse.AccessToken);
            Assert.Equal("rotated-refresh-token", refreshResponse.RefreshToken);
            Assert.Equal("rotated-refresh-token", user.RefreshToken);
            userManager.Verify(manager => manager.UpdateAsync(user), Times.Exactly(2));
        }

        private static AuthService CreateAuthService(
            Mock<UserManager<ApplicationUser>> userManager,
            Mock<ITokenRepository>? tokenRepository = null,
            Mock<ITourGuideRepository>? tourGuideRepository = null)
        {
            return new AuthService(
                userManager.Object,
                (tokenRepository ?? new Mock<ITokenRepository>()).Object,
                (tourGuideRepository ?? new Mock<ITourGuideRepository>()).Object);
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

        private static ClaimsPrincipal CreatePrincipal(string email)
        {
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Email, email)
            });

            return new ClaimsPrincipal(identity);
        }
    }
}
