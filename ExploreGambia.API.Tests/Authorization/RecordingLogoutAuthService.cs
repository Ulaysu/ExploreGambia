using ExploreGambia.API.Models.DTOs;
using ExploreGambia.API.Services;

namespace ExploreGambia.API.Tests.Authorization
{
    internal sealed class RecordingLogoutAuthService : IAuthService
    {
        public int LogoutCalls { get; private set; }

        public string? LastLoggedOutUserId { get; private set; }

        public Task<RegisterResponseDto> RegisterAsync(RegisterRequestDto registerRequestDto)
        {
            return Task.FromResult(new RegisterResponseDto
            {
                IsSuccess = true,
                Message = "User was registered successfully! Please login."
            });
        }

        public Task<LoginResponseDto> LoginAsync(LoginRequestDto loginRequestDto)
        {
            return Task.FromResult(new LoginResponseDto
            {
                JwtToken = "authorization-test-access-token",
                RefreshToken = "authorization-test-refresh-token"
            });
        }

        public Task<RefreshTokenResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request)
        {
            return Task.FromResult(new RefreshTokenResponseDto
            {
                AccessToken = "authorization-test-rotated-access-token",
                RefreshToken = "authorization-test-rotated-refresh-token"
            });
        }

        public Task LogoutAsync(string userId)
        {
            LogoutCalls++;
            LastLoggedOutUserId = userId;

            return Task.CompletedTask;
        }
    }
}
