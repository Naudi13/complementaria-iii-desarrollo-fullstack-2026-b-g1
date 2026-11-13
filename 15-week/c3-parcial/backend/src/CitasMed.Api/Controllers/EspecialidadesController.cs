using CitasMed.Application.Features.Especialidades;
using Microsoft.AspNetCore.Mvc;

namespace CitasMed.Api.Controllers;

/// <summary>Catálogo de especialidades. Requiere usuario autenticado (política global).</summary>
[ApiController]
[Route("api/especialidades")]
[Produces("application/json")]
public class EspecialidadesController(IEspecialidadService service) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<EspecialidadDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<EspecialidadDto>>> GetAll(CancellationToken ct)
        => Ok(await service.GetAllAsync(ct));
}
