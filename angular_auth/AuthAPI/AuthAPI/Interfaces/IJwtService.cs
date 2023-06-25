using AuthAPI.Models;
using System.Security.Claims;

namespace AuthAPI.Interfaces
{
    public interface IJwtService
    {
        public string CreateJwt(User userModdel);
        public ClaimsPrincipal GetPrincipleFromExpiredToken(string token);
    }
}
