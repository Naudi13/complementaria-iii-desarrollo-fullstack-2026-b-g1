namespace CitasMed.Api.Extensions;

public static class CorsExtensions
{
    public const string FrontendPolicy = "Frontend";

    /// <summary>
    /// CORS: el navegador bloquea las respuestas de un origen distinto (localhost:4200 → localhost:5142)
    /// salvo que la API declare ese origen como permitido.
    /// Se usa una lista blanca leída de configuración (Cors:AllowedOrigins, separada por comas,
    /// o la variable de entorno Cors__AllowedOrigins). Nunca AllowAnyOrigin en producción.
    /// </summary>
    public static IServiceCollection AddFrontendCors(this IServiceCollection services, IConfiguration configuration)
    {
        var origins = (configuration["Cors:AllowedOrigins"] ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        services.AddCors(options => options.AddPolicy(FrontendPolicy, policy =>
            policy.WithOrigins(origins)
                  .WithHeaders("Authorization", "Content-Type")
                  .WithMethods("GET", "POST", "PUT", "PATCH", "DELETE")));

        return services;
    }
}
