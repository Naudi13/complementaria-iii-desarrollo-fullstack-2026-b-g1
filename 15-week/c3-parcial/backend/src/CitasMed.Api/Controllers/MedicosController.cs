using CitasMed.Application.Features.Medicos;
using CitasMed.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CitasMed.Api.Controllers;

/// <summary>
/// Consulta: cualquier usuario autenticado. Crear/eliminar: solo Admin.
/// El controlador es delgado: recibe, delega en el servicio y devuelve.
/// </summary>
[ApiController]
[Route("api/medicos")]
[Produces("application/json")]
public class MedicosController(IMedicoService service) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<MedicoDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<MedicoDto>>> GetAll([FromQuery] int? especialidadId, CancellationToken ct)
        => Ok(await service.GetAllAsync(especialidadId, ct));

    [HttpGet("{id:int}")]
    [ProducesResponseType<MedicoDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MedicoDto>> GetById(int id, CancellationToken ct)
        => Ok(await service.GetByIdAsync(id, ct));

    [HttpPost]
    [Authorize(Roles = nameof(Rol.Admin))]
    [ProducesResponseType<MedicoDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<MedicoDto>> Crear(CrearMedicoRequest request, CancellationToken ct)
    {
        var medico = await service.CrearAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = medico.Id }, medico);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = nameof(Rol.Admin))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Eliminar(int id, CancellationToken ct)
    {
        await service.EliminarAsync(id, ct);
        return NoContent();
    }
}
