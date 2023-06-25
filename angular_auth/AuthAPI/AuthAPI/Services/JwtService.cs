using AuthAPI.Context;
using AuthAPI.Interfaces;
using AuthAPI.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace AuthAPI.Services
{
    public class JwtService : IJwtService
    {
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _dbContext;
        public JwtService(IConfiguration configuration,AppDbContext dbContext)
        {
            _dbContext = dbContext;
            _configuration = configuration;
        }

        public string CreateJwt(User userModel)
        {

            var jwtTokenHandler = new JwtSecurityTokenHandler();

            var secretKey = GetSecretKey();

            var identity = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Role, userModel.Role),
                new Claim(ClaimTypes.Name, $"{userModel.UserName}")
            });

            var credentials = new SigningCredentials(new SymmetricSecurityKey(secretKey), SecurityAlgorithms.HmacSha256);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = identity,
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = credentials,
            };
            var token = jwtTokenHandler.CreateToken(tokenDescriptor);

            return jwtTokenHandler.WriteToken(token);

        }
        private string CreateRefreshToken()
        {
            var tokenBytes = RandomNumberGenerator.GetBytes(64);
            var refresjToken = Convert.ToBase64String(tokenBytes);

            var tokenInUser = _dbContext.Users
                .Any(a=>a.RefreshToken == refresjToken);
            if(tokenInUser)
            {
                return CreateRefreshToken();
            }
            return refresjToken;
        }
        public ClaimsPrincipal GetPrincipleFromExpiredToken(string token)
        {
            var key = GetSecretKey();
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateLifetime = false,
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken;

            var principal = tokenHandler.ValidateToken(token,tokenValidationParameters,out securityToken);
            var jwtSecurityToken = securityToken as JwtSecurityToken;
            if(jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("This is Invalid Token");
            }
            return principal;
            
        }
        private byte[] GetSecretKey()
        {
            string secretKey = _configuration["Jwt:SecretKey"];
            return Encoding.ASCII.GetBytes(secretKey);
        }
    }
}
