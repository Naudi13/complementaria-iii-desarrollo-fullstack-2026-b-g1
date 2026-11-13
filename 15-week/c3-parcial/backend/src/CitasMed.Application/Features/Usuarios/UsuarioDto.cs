using CitasMed.Domain.Entities;

namespace CitasMed.Application.Features.Usuarios;

/// <summary>
/// Datos públicos del usuario. Nunca expone el PasswordHash.
/// </summary>
public record UsuarioDto(int Id, string Nombre, string Email, string Rol)
{
    public static UsuarioDto From(User user) =>
        new(user.Id, user.Nombre, user.Email, user.Rol.ToString());
}
