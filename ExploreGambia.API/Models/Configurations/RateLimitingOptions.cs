namespace ExploreGambia.API.Models.Configurations
{
    public class RateLimitingOptions
    {
        public const string SectionName = "RateLimiting";

        public RateLimitRuleOptions Login { get; set; } = new()
        {
            PermitLimit = 5,
            WindowSeconds = 60
        };

        public RateLimitRuleOptions Registration { get; set; } = new()
        {
            PermitLimit = 3,
            WindowSeconds = 600
        };

        public RateLimitRuleOptions RefreshToken { get; set; } = new()
        {
            PermitLimit = 10,
            WindowSeconds = 60
        };
    }
}
