using CitasMed.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CitasMed.Infrastructure.Persistence;

public static class DatabaseInitializer
{
    /// <summary>
    /// Aplica las migraciones pendientes y carga los datos de prueba.
    /// Se invoca al arrancar la Api en entorno Development.
    /// </summary>
    public static async Task InitializeDatabaseAsync(this IServiceProvider services, CancellationToken ct = default)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        await db.Database.MigrateAsync(ct);
        await DbSeeder.SeedAsync(db, hasher, ct);
    }
}
