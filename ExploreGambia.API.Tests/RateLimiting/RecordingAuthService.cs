using ExploreGambia.API.Models.DTOs;
using ExploreGambia.API.Services;

namespace ExploreGambia.API.Tests.RateLimiting
{
    internal sealed class RecordingAuthService : IAuthService
    {
        public int LoginCalls { get; private set; }

        public int RegisterCalls { get; private set; }

        public int RefreshTokenCalls { get; private set; }

        public int LogoutCalls { get; private set; }

        public Task<RegisterResponseDto> RegisterAsync(RegisterRequestDto registerRequestDto)
        {
            RegisterCalls++;

            return Task.FromResult(new RegisterResponseDto
            {
                IsSuccess = true,
                Message = "User was registered successfully! Please login."
            });
        }

        public Task<LoginResponseDto> LoginAsync(LoginRequestDto loginRequestDto)
        {
            LoginCalls++;

            return Task.FromResult(new LoginResponseDto
            {
                JwtToken = "test-access-token",
                RefreshToken = "test-refresh-token"
            });
        }

        public Task<RefreshTokenResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request)
        {
            RefreshTokenCalls++;

            return Task.FromResult(new RefreshTokenResponseDto
            {
                AccessToken = "rotated-access-token",
                RefreshToken = "rotated-refresh-token"
            });
        }

        public Task LogoutAsync(string userId)
        {
            LogoutCalls++;

            return Task.CompletedTask;
        }
    }
}
