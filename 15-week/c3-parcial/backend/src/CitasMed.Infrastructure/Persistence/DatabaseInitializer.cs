using CitasMed.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CitasMed.Infrastructure.Persistence;

public static class DatabaseInitializer
{
    /// <summary>
    /// Aplica las migraciones pendientes y, si se indica, carga los datos de demostración.
    /// En Development se ejecuta siempre; en otros entornos se controla con
    /// Database__MigrateOnStartup y Database__SeedDemoData.
    /// </summary>
    public static async Task InitializeDatabaseAsync(this IServiceProvider services, bool seedDemoData, CancellationToken ct = default)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await db.Database.MigrateAsync(ct);

        if (seedDemoData)
        {
            var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
            await DbSeeder.SeedAsync(db, hasher, ct);
        }
    }
}
