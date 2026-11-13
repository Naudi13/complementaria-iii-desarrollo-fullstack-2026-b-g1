using CitasMed.Application.Common.Interfaces;
using CitasMed.Domain.Entities;
using CitasMed.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CitasMed.Infrastructure.Persistence;

/// <summary>
/// Datos iniciales SOLO para desarrollo y pruebas.
/// Las contraseñas de abajo son de prueba; nunca uses estas cuentas en producción.
/// </summary>
public static class DbSeeder
{
    public const string DevPassword = "CitasMed2026*";

    public static async Task SeedAsync(AppDbContext db, IPasswordHasher hasher, CancellationToken ct = default)
    {
        if (await db.Users.AnyAsync(ct))
            return; // ya hay datos

        var hash = hasher.Hash(DevPassword);

        var general = new Especialidad { Nombre = "Medicina General" };
        var pediatria = new Especialidad { Nombre = "Pediatría" };
        db.Especialidades.AddRange(general, pediatria);

        var admin = new User { Nombre = "Administrador", Email = "admin@citasmed.test", PasswordHash = hash, Rol = Rol.Admin };
        var paciente = new User { Nombre = "Paciente Demo", Email = "paciente@citasmed.test", PasswordHash = hash, Rol = Rol.Paciente };
        var paciente2 = new User { Nombre = "Paciente Dos", Email = "paciente2@citasmed.test", PasswordHash = hash, Rol = Rol.Paciente };

        var medico1 = new Medico
        {
            RegistroProfesional = "RM-1001",
            Especialidad = general,
            User = new User { Nombre = "Dra. Laura Gómez", Email = "laura.gomez@citasmed.test", PasswordHash = hash, Rol = Rol.Medico }
        };
        var medico2 = new Medico
        {
            RegistroProfesional = "RM-1002",
            Especialidad = pediatria,
            User = new User { Nombre = "Dr. Andrés Rojas", Email = "andres.rojas@citasmed.test", PasswordHash = hash, Rol = Rol.Medico }
        };

        db.Users.AddRange(admin, paciente, paciente2);
        db.Medicos.AddRange(medico1, medico2);

        await db.SaveChangesAsync(ct);
    }
}
