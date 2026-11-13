using CitasMed.Application.Common.Interfaces;
using CitasMed.Application.Features.Usuarios;
using CitasMed.Infrastructure.Persistence;
using CitasMed.Infrastructure.Repositories;
using CitasMed.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CitasMed.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Registra persistencia (EF Core), repositorios y servicios técnicos (hashing, JWT).
    /// Los valores sensibles (cadena de conexión, clave JWT) se leen de configuración /
    /// variables de entorno, nunca van quemados en el código.
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // ---------- Persistencia ----------
        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Falta la cadena de conexión 'ConnectionStrings:Default'.");

        services.AddDbContext<AppDbContext>(options => options.UseSqlite(connectionString));

        services.AddScoped<IUserRepository, UserRepository>();

        // ---------- Seguridad ----------
        // ValidateOnStart: si falta la clave JWT o es muy corta, la app no arranca (falla rápido).
        services.AddOptions<JwtOptions>()
                .Bind(configuration.GetSection(JwtOptions.SectionName))
                .ValidateDataAnnotations()
                .ValidateOnStart();

        services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
        services.AddSingleton<IJwtService, JwtService>();

        return services;
    }
}
