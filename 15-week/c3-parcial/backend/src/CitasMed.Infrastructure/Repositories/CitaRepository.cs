using CitasMed.Application.Features.Citas;
using CitasMed.Domain.Entities;
using CitasMed.Domain.Enums;
using CitasMed.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CitasMed.Infrastructure.Repositories;

public class CitaRepository(AppDbContext db) : ICitaRepository
{
    private IQueryable<Cita> ConDetalle =>
        db.Citas
          .Include(c => c.Paciente)
          .Include(c => c.Medico).ThenInclude(m => m.User)
          .Include(c => c.Medico).ThenInclude(m => m.Especialidad);

    public Task<Cita?> GetByIdAsync(int id, CancellationToken ct = default) =>
        ConDetalle.FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<IReadOnlyList<Cita>> GetAllAsync(CancellationToken ct = default) =>
        await ConDetalle.AsNoTracking().OrderBy(c => c.FechaHora).ToListAsync(ct);

    public async Task<IReadOnlyList<Cita>> GetByPacienteAsync(int pacienteId, CancellationToken ct = default) =>
        await ConDetalle.AsNoTracking().Where(c => c.PacienteId == pacienteId)
                        .OrderBy(c => c.FechaHora).ToListAsync(ct);

    public async Task<IReadOnlyList<Cita>> GetByMedicoAsync(int medicoId, CancellationToken ct = default) =>
        await ConDetalle.AsNoTracking().Where(c => c.MedicoId == medicoId)
                        .OrderBy(c => c.FechaHora).ToListAsync(ct);

    public Task<bool> HorarioOcupadoAsync(int medicoId, DateTime fechaHora, CancellationToken ct = default) =>
        db.Citas.AnyAsync(c => c.MedicoId == medicoId
                            && c.FechaHora == fechaHora
                            && c.Estado != EstadoCita.Cancelada, ct);

    public void Add(Cita cita) => db.Citas.Add(cita);
}
