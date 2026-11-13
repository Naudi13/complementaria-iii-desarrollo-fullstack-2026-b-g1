using CitasMed.Application.Features.Usuarios;
using CitasMed.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CitasMed.Api.Controllers;

/// <summary>
/// Autorización por rol: además de estar autenticado, el token debe traer role = Admin.
/// Sin token → 401 Unauthorized · Token de Paciente/Medico → 403 Forbidden.
/// </summary>
[ApiController]
[Route("api/usuarios")]
[Produces("application/json")]
[Authorize(Roles = nameof(Rol.Admin))]
public class UsuariosController(IUsuarioService usuarioService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<UsuarioDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IReadOnlyList<UsuarioDto>>> GetAll(CancellationToken ct)
        => Ok(await usuarioService.GetAllAsync(ct));
}
