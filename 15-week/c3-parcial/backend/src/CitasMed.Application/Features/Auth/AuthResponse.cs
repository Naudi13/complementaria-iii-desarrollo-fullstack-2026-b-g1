using CitasMed.Application.Features.Usuarios;

namespace CitasMed.Application.Features.Auth;

/// <summary>
/// Respuesta del login. El frontend guarda AccessToken y lo envía en cada
/// petición como: Authorization: Bearer {AccessToken}
/// </summary>
public record AuthResponse(string AccessToken, string TokenType, DateTime ExpiresAt, UsuarioDto Usuario);
