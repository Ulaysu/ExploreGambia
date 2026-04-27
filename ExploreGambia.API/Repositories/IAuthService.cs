using ExploreGambia.API.Models.DTOs;

namespace ExploreGambia.API.Repositories
{
    public interface IAuthService
    {
        Task<RegisterResponseDto> RegisterAsync(RegisterRequestDto registerRequestDto);
        Task<LoginResponseDto> LoginAsync(LoginRequestDto loginRequestDto);
    }
}   