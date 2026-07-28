using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using UserService.Domain.Entities;

namespace UserService.Services
{
    public interface ITokenService
    {
        string GenerateToken(User user);
    }

    /// <summary>
    /// Issues the JWT described in Chapter 4.2: identifier, role, and expiry,
    /// signed with HMACSHA256 using a key shared (via .NET Aspire configuration)
    /// with every other service, since each service validates the token itself —
    /// there is no central API Gateway to do it once.
    /// </summary>
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;

        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Type.ToString()),
            };

            var signingKey = _configuration["Jwt:SigningKey"]
                ?? throw new InvalidOperationException("Jwt:SigningKey is not configured.");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expiresMinutes = int.TryParse(_configuration["Jwt:ExpiresMinutes"], out var m) ? m : 120;

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiresMinutes),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
