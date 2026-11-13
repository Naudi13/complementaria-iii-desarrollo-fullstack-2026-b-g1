using CitasMed.Application.Features.Citas;
using CitasMed.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CitasMed.Api.Controllers;

/// <summary>
/// Dos niveles de autorización:
/// 1) Por ROL en el controlador: [Authorize(Roles = ...)] → 403 si el rol no corresponde.
/// 2) Por RECURSO en el servicio: la cita debe pertenecer al usuario → 403 si no es suya.
/// </summary>
[ApiController]
[Route("api/citas")]
[Produces("application/json")]
public class CitasController(ICitaService service) : ControllerBase
{
    /// <summary>Todas las citas (administración).</summary>
    [HttpGet]
    [Authorize(Roles = nameof(Rol.Admin))]
    [ProducesResponseType<IReadOnlyList<CitaDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CitaDto>>> GetAll(CancellationToken ct)
        => Ok(await service.GetAllAsync(ct));

    /// <summary>Citas del paciente autenticado (filtradas por el claim "sub").</summary>
    [HttpGet("mias")]
    [Authorize(Roles = nameof(Rol.Paciente))]
    [ProducesResponseType<IReadOnlyList<CitaDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CitaDto>>> GetMias(CancellationToken ct)
        => Ok(await service.GetMiasAsync(ct));

    /// <summary>Agenda del médico autenticado.</summary>
    [HttpGet("agenda")]
    [Authorize(Roles = nameof(Rol.Medico))]
    [ProducesResponseType<IReadOnlyList<CitaDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CitaDto>>> GetAgenda(CancellationToken ct)
        => Ok(await service.GetAgendaAsync(ct));

    /// <summary>Reservar una cita (paciente).</summary>
    [HttpPost]
    [Authorize(Roles = nameof(Rol.Paciente))]
    [ProducesResponseType<CitaDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CitaDto>> Crear(CrearCitaRequest request, CancellationToken ct)
    {
        var cita = await service.CrearAsync(request, ct);
        return Created($"api/citas/{cita.Id}", cita);
    }

    /// <summary>Cancelar una cita propia (paciente) o cualquiera (admin).</summary>
    [HttpPatch("{id:int}/cancelar")]
    [Authorize(Roles = $"{nameof(Rol.Paciente)},{nameof(Rol.Admin)}")]
    [ProducesResponseType<CitaDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CitaDto>> Cancelar(int id, CancellationToken ct)
        => Ok(await service.CancelarAsync(id, ct));

    /// <summary>Marcar como atendida (solo el médico asignado).</summary>
    [HttpPatch("{id:int}/atender")]
    [Authorize(Roles = nameof(Rol.Medico))]
    [ProducesResponseType<CitaDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CitaDto>> Atender(int id, CancellationToken ct)
        => Ok(await service.AtenderAsync(id, ct));
}
