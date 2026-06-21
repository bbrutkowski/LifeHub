using LifeHub.Application.Interfaces.Services;
using LifeHub.Domain.Entity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace LifeHub.Infrastructure.Security
{
    public class JwtTokenService : ITokenService
    {
        private readonly IConfiguration _configuration;
        private readonly byte[] _key;

        public JwtTokenService(IConfiguration configuration)
        {
            _configuration = configuration;
            var keyString = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key not configured in configuration");
            _key = Encoding.UTF8.GetBytes(keyString);
        }

        public Task<(string Token, DateTime ExpiresAt)> GenerateToken(User user)
        {
            var issuer = _configuration["Jwt:Issuer"] ?? "LifeHub";
            var audience = _configuration["Jwt:Audience"] ?? "LifeHubClients";
            var expiresMinutes = int.TryParse(_configuration["Jwt:ExpiresMinutes"], out var m) ? m : 60;

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
                new Claim(JwtRegisteredClaimNames.Email, user.Email)
            };

            var expires = DateTime.UtcNow.AddMinutes(expiresMinutes);
            var creds = new SigningCredentials(new SymmetricSecurityKey(_key), SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: expires,
                signingCredentials: creds);

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            return Task.FromResult((tokenString, expires));
        }
    }
}
