using CitasMed.Application.Common.Exceptions;
using CitasMed.Application.Common.Interfaces;
using CitasMed.Application.Features.Usuarios;

namespace CitasMed.Application.Features.Auth;

public interface IAuthService
{
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default);
}

/// <summary>
/// Autenticación: verifica QUIÉN es el usuario (email + contraseña) y emite el JWT.
/// La autorización (QUÉ puede hacer) ocurre después, en cada endpoint, usando los claims del token.
/// </summary>
public class AuthService(IUserRepository users, IPasswordHasher hasher, IJwtService jwt) : IAuthService
{
    // Mismo mensaje para "no existe" y "contraseña incorrecta":
    // así no se revela qué emails están registrados.
    private const string CredencialesInvalidas = "Email o contraseña incorrectos.";

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var user = await users.GetByEmailAsync(email, ct);
        if (user is null || !hasher.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedException(CredencialesInvalidas);

        var token = jwt.Generate(user);

        return new AuthResponse(token.AccessToken, "Bearer", token.ExpiresAt, UsuarioDto.From(user));
    }
}
