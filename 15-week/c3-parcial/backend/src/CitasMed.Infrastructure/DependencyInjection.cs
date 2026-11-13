using CitasMed.Application.Common.Interfaces;
using CitasMed.Infrastructure.Persistence;
using CitasMed.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CitasMed.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Registra persistencia (EF Core) y servicios técnicos (hashing, JWT en fase 2).
    /// La cadena de conexión se lee de configuración / variables de entorno
    /// (ConnectionStrings__Default), nunca va quemada en el código.
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Falta la cadena de conexión 'ConnectionStrings:Default'.");

        services.AddDbContext<AppDbContext>(options => options.UseSqlite(connectionString));

        services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();

        return services;
    }
}
