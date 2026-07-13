namespace ExploreGambia.API.Models.Configurations
{
    public class RateLimitRuleOptions
    {
        public int PermitLimit { get; set; }

        public int WindowSeconds { get; set; }
    }
}
