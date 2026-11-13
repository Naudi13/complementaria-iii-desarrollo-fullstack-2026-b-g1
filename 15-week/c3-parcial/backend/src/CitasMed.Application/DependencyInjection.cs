using Microsoft.Extensions.DependencyInjection;

namespace CitasMed.Application;

public static class DependencyInjection
{
    /// <summary>
    /// Registra los servicios de la capa Application (casos de uso).
    /// Cada feature (Auth, Medicos, Citas) agregará aquí sus servicios.
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Fase 2: services.AddScoped<IAuthService, AuthService>();
        // Fase 3: services.AddScoped<IMedicoService, MedicoService>();
        //         services.AddScoped<ICitaService, CitaService>();
        return services;
    }
}
