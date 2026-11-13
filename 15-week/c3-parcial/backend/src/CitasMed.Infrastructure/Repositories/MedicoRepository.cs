using CitasMed.Application.Features.Medicos;
using CitasMed.Domain.Entities;
using CitasMed.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CitasMed.Infrastructure.Repositories;

public class MedicoRepository(AppDbContext db) : IMedicoRepository
{
    private IQueryable<Medico> ConDetalle =>
        db.Medicos.Include(m => m.User).Include(m => m.Especialidad);

    public async Task<IReadOnlyList<Medico>> GetAllAsync(int? especialidadId = null, CancellationToken ct = default)
    {
        var query = ConDetalle.AsNoTracking();
        if (especialidadId is not null)
            query = query.Where(m => m.EspecialidadId == especialidadId);

        return await query.OrderBy(m => m.User.Nombre).ToListAsync(ct);
    }

    public Task<Medico?> GetByIdAsync(int id, CancellationToken ct = default) =>
        ConDetalle.FirstOrDefaultAsync(m => m.Id == id, ct);

    public Task<Medico?> GetByUserIdAsync(int userId, CancellationToken ct = default) =>
        db.Medicos.AsNoTracking().FirstOrDefaultAsync(m => m.UserId == userId, ct);

    public Task<bool> ExistsAsync(int id, CancellationToken ct = default) =>
        db.Medicos.AnyAsync(m => m.Id == id, ct);

    public Task<bool> ExistsRegistroAsync(string registroProfesional, CancellationToken ct = default) =>
        db.Medicos.AnyAsync(m => m.RegistroProfesional == registroProfesional, ct);

    public Task<bool> TieneCitasAsync(int medicoId, CancellationToken ct = default) =>
        db.Citas.AnyAsync(c => c.MedicoId == medicoId, ct);

    public void Add(Medico medico) => db.Medicos.Add(medico);

    // Al borrar el usuario, EF elimina el perfil de médico en cascada.
    public void Remove(Medico medico) => db.Users.Remove(medico.User);
}
