using ExploreGambia.API.Models.DTOs;

namespace ExploreGambia.API.Services
{
    public interface IAuthService
    {
        Task<RegisterResponseDto> RegisterAsync(RegisterRequestDto registerRequestDto);
        Task<LoginResponseDto> LoginAsync(LoginRequestDto loginRequestDto);

        Task<RefreshTokenResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request);

        Task LogoutAsync(string userId);
    }
}   
