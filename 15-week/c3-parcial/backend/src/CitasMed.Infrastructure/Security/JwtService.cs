using System.Text;
using CitasMed.Application.Common.Interfaces;
using CitasMed.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace CitasMed.Infrastructure.Security;

/// <summary>
/// Emite el JWT firmado con HMAC-SHA256.
/// Header: alg/typ · Payload: claims (sub, email, name, role, exp...) · Firma: HMAC(header.payload, Key).
/// Ojo: el payload va en Base64Url, NO cifrado; nunca se ponen datos sensibles en él.
/// </summary>
public class JwtService(IOptions<JwtOptions> options) : IJwtService
{
    private readonly JwtOptions _options = options.Value;
    private readonly JsonWebTokenHandler _handler = new();

    public TokenResult Generate(User user)
    {
        var now = DateTime.UtcNow;
        var expires = now.AddMinutes(_options.ExpiresMinutes);
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key));

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = _options.Issuer,
            Audience = _options.Audience,
            IssuedAt = now,
            NotBefore = now,
            Expires = expires,
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256),
            Claims = new Dictionary<string, object>
            {
                [JwtRegisteredClaimNames.Sub] = user.Id.ToString(),
                [JwtRegisteredClaimNames.Email] = user.Email,
                [JwtRegisteredClaimNames.Name] = user.Nombre,
                [JwtRegisteredClaimNames.Jti] = Guid.NewGuid().ToString(),
                ["role"] = user.Rol.ToString()
            }
        };

        return new TokenResult(_handler.CreateToken(descriptor), expires);
    }
}
