namespace ExploreGambia.API.Models.DTOs
{
    public class LoginResponseDto
    {
        public string JwtToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public string? Error { get; set; }
    }
}
