using CitasMed.Domain.Entities;

namespace CitasMed.Application.Features.Citas;

public interface ICitaRepository
{
    /// <summary>Obtiene la cita con Paciente y Medico cargados (con seguimiento para modificarla).</summary>
    Task<Cita?> GetByIdAsync(int id, CancellationToken ct = default);

    Task<IReadOnlyList<Cita>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Cita>> GetByPacienteAsync(int pacienteId, CancellationToken ct = default);
    Task<IReadOnlyList<Cita>> GetByMedicoAsync(int medicoId, CancellationToken ct = default);

    /// <summary>True si el médico ya tiene una cita no cancelada en esa fecha y hora.</summary>
    Task<bool> HorarioOcupadoAsync(int medicoId, DateTime fechaHora, CancellationToken ct = default);

    void Add(Cita cita);
}
