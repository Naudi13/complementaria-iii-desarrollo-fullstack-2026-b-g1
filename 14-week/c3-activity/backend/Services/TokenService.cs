// Services/TokenService.cs
// Genera el JWT firmado (HS256) que el frontend enviará en cada petición.
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Globalization;
using System.Text;
using Backend.Api.Models;
using Microsoft.IdentityModel.Tokens;

namespace Backend.Api.Services;

public class TokenService
{
    private readonly IConfiguration _config;

    public TokenService(IConfiguration config) => _config = config;

    public (string Token, DateTime ExpiresAt) CreateToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(
            double.Parse(_config["Jwt:ExpireMinutes"] ?? "60", CultureInfo.InvariantCulture));

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: expires,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expires);
    }
}
