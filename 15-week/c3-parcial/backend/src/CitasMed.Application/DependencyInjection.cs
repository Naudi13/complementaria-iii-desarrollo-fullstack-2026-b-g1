using CitasMed.Application.Features.Auth;
using CitasMed.Application.Features.Citas;
using CitasMed.Application.Features.Especialidades;
using CitasMed.Application.Features.Medicos;
using CitasMed.Application.Features.Usuarios;
using Microsoft.Extensions.DependencyInjection;

namespace CitasMed.Application;

public static class DependencyInjection
{
    /// <summary>
    /// Registra los servicios de la capa Application (casos de uso), uno por feature.
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System); // reloj inyectable → pruebas deterministas

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUsuarioService, UsuarioService>();
        services.AddScoped<IEspecialidadService, EspecialidadService>();
        services.AddScoped<IMedicoService, MedicoService>();
        services.AddScoped<ICitaService, CitaService>();

        return services;
    }
}
