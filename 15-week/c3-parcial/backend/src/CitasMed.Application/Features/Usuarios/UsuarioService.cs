using CitasMed.Application.Common.Exceptions;
using CitasMed.Application.Common.Interfaces;

namespace CitasMed.Application.Features.Usuarios;

public interface IUsuarioService
{
    /// <summary>Perfil del usuario autenticado (a partir del token).</summary>
    Task<UsuarioDto> GetActualAsync(CancellationToken ct = default);

    Task<IReadOnlyList<UsuarioDto>> GetAllAsync(CancellationToken ct = default);
}

public class UsuarioService(IUserRepository users, ICurrentUser currentUser) : IUsuarioService
{
    public async Task<UsuarioDto> GetActualAsync(CancellationToken ct = default)
    {
        var id = currentUser.Id ?? throw new UnauthorizedException("No hay un usuario autenticado.");

        var user = await users.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("El usuario del token ya no existe.");

        return UsuarioDto.From(user);
    }

    public async Task<IReadOnlyList<UsuarioDto>> GetAllAsync(CancellationToken ct = default)
    {
        var list = await users.GetAllAsync(ct);
        return list.Select(UsuarioDto.From).ToList();
    }
}
