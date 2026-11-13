using CitasMed.Application.Features.Usuarios;
using CitasMed.Domain.Entities;
using CitasMed.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CitasMed.Infrastructure.Repositories;

public class UserRepository(AppDbContext db) : IUserRepository
{
    public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default) =>
        db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email, ct);

    public Task<User?> GetByIdAsync(int id, CancellationToken ct = default) =>
        db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id, ct);

    public async Task<IReadOnlyList<User>> GetAllAsync(CancellationToken ct = default) =>
        await db.Users.AsNoTracking().OrderBy(u => u.Nombre).ToListAsync(ct);

    public Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default) =>
        db.Users.AnyAsync(u => u.Email == email, ct);
}
