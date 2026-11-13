using System.ComponentModel.DataAnnotations;

namespace CitasMed.Infrastructure.Security;

/// <summary>
/// Configuración del JWT (sección "Jwt"). En producción se entrega por
/// variables de entorno: Jwt__Key, Jwt__Issuer, Jwt__Audience, Jwt__ExpiresMinutes.
/// </summary>
public class JwtOptions
{
    public const string SectionName = "Jwt";

    /// <summary>Clave secreta HMAC-SHA256. Mínimo 32 caracteres (256 bits).</summary>
    [Required, MinLength(32)]
    public string Key { get; set; } = string.Empty;

    [Required]
    public string Issuer { get; set; } = string.Empty;

    [Required]
    public string Audience { get; set; } = string.Empty;

    [Range(1, 1440)]
    public int ExpiresMinutes { get; set; } = 60;
}
