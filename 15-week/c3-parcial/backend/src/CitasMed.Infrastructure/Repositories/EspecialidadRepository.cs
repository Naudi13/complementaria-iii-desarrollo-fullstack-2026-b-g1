using CitasMed.Application.Features.Especialidades;
using CitasMed.Domain.Entities;
using CitasMed.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CitasMed.Infrastructure.Repositories;

public class EspecialidadRepository(AppDbContext db) : IEspecialidadRepository
{
    public async Task<IReadOnlyList<Especialidad>> GetAllAsync(CancellationToken ct = default) =>
        await db.Especialidades.AsNoTracking().OrderBy(e => e.Nombre).ToListAsync(ct);

    public Task<Especialidad?> GetByIdAsync(int id, CancellationToken ct = default) =>
        db.Especialidades.FirstOrDefaultAsync(e => e.Id == id, ct);
}
