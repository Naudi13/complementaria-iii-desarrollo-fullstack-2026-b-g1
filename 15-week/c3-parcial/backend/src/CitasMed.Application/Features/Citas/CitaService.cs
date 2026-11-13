using CitasMed.Application.Common.Exceptions;
using CitasMed.Application.Common.Interfaces;
using CitasMed.Application.Features.Medicos;
using CitasMed.Domain.Entities;
using CitasMed.Domain.Enums;

namespace CitasMed.Application.Features.Citas;

public interface ICitaService
{
    Task<IReadOnlyList<CitaDto>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<CitaDto>> GetMiasAsync(CancellationToken ct = default);
    Task<IReadOnlyList<CitaDto>> GetAgendaAsync(CancellationToken ct = default);
    Task<CitaDto> CrearAsync(CrearCitaRequest request, CancellationToken ct = default);
    Task<CitaDto> CancelarAsync(int id, CancellationToken ct = default);
    Task<CitaDto> AtenderAsync(int id, CancellationToken ct = default);
}

/// <summary>
/// Casos de uso de citas. El rol ya lo filtra el controlador ([Authorize(Roles=...)]);
/// aquí se aplica la autorización POR RECURSO: que la cita pertenezca a quien la modifica.
/// </summary>
public class CitaService(
    ICitaRepository citas,
    IMedicoRepository medicos,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : ICitaService
{
    public async Task<IReadOnlyList<CitaDto>> GetAllAsync(CancellationToken ct = default) =>
        Map(await citas.GetAllAsync(ct));

    public async Task<IReadOnlyList<CitaDto>> GetMiasAsync(CancellationToken ct = default) =>
        Map(await citas.GetByPacienteAsync(UsuarioId(), ct));

    public async Task<IReadOnlyList<CitaDto>> GetAgendaAsync(CancellationToken ct = default)
    {
        var medico = await MedicoActualAsync(ct);
        return Map(await citas.GetByMedicoAsync(medico.Id, ct));
    }

    public async Task<CitaDto> CrearAsync(CrearCitaRequest request, CancellationToken ct = default)
    {
        var pacienteId = UsuarioId();

        if (!await medicos.ExistsAsync(request.MedicoId, ct))
            throw new NotFoundException($"No existe el médico {request.MedicoId}.");

        var ahora = timeProvider.GetLocalNow().DateTime;
        var cita = Cita.Programar(pacienteId, request.MedicoId, request.FechaHora, request.Motivo, ahora);

        if (await citas.HorarioOcupadoAsync(cita.MedicoId, cita.FechaHora, ct))
            throw new ConflictException("El médico ya tiene una cita en ese horario.");

        citas.Add(cita);
        await unitOfWork.SaveChangesAsync(ct);

        return await GetDtoAsync(cita.Id, ct);
    }

    public async Task<CitaDto> CancelarAsync(int id, CancellationToken ct = default)
    {
        var cita = await GetCitaAsync(id, ct);

        // El Admin puede cancelar cualquier cita; el paciente solo las suyas.
        if (currentUser.Rol != nameof(Rol.Admin) && !cita.PerteneceA(UsuarioId()))
            throw new ForbiddenException("Solo puedes cancelar tus propias citas.");

        cita.Cancelar();
        await unitOfWork.SaveChangesAsync(ct);
        return CitaDto.From(cita);
    }

    public async Task<CitaDto> AtenderAsync(int id, CancellationToken ct = default)
    {
        var cita = await GetCitaAsync(id, ct);
        var medico = await MedicoActualAsync(ct);

        if (cita.MedicoId != medico.Id)
            throw new ForbiddenException("Solo el médico asignado puede atender esta cita.");

        cita.Atender();
        await unitOfWork.SaveChangesAsync(ct);
        return CitaDto.From(cita);
    }

    // ---------- helpers ----------

    private int UsuarioId() =>
        currentUser.Id ?? throw new UnauthorizedException("No hay un usuario autenticado.");

    private async Task<Medico> MedicoActualAsync(CancellationToken ct) =>
        await medicos.GetByUserIdAsync(UsuarioId(), ct)
        ?? throw new ForbiddenException("El usuario actual no tiene perfil de médico.");

    private async Task<Cita> GetCitaAsync(int id, CancellationToken ct) =>
        await citas.GetByIdAsync(id, ct) ?? throw new NotFoundException($"No existe la cita {id}.");

    private async Task<CitaDto> GetDtoAsync(int id, CancellationToken ct) =>
        CitaDto.From(await GetCitaAsync(id, ct));

    private static IReadOnlyList<CitaDto> Map(IEnumerable<Cita> list) =>
        list.Select(CitaDto.From).ToList();
}
