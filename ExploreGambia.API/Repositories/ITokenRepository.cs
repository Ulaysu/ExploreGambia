using Microsoft.AspNetCore.Identity;

namespace ExploreGambia.API.Repositories
{
    public interface ITokenRepository
    {
        string CreateJWTToken(IdentityUser user, List<string> roles);
    }
}
