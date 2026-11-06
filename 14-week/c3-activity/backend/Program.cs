// Program.cs
// Punto de entrada del backend. Configura:
//   1. Autenticación JWT (Bearer) y autorización.
//   2. CORS, para que el frontend Angular (otro origen) pueda llamar a la API.
//   3. Repositorios en memoria como Singleton.
using System.Text;
using Backend.Api.Data;
using Backend.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// ---------- Configuración JWT ----------
// La clave NUNCA debe quedar en el repositorio en producción:
// se sobreescribe con la variable de entorno Jwt__Key (ver Dockerfile/README).
var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(jwtKey) || jwtKey.Length < 32)
{
    throw new InvalidOperationException(
        "Jwt:Key debe existir y tener al menos 32 caracteres (HS256).");
}

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.FromSeconds(30),
        };
    });

builder.Services.AddAuthorization();

// ---------- CORS ----------
// Solo se permiten los orígenes listados en Cors:AllowedOrigins (no "cualquiera").
var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()   // incluye Authorization y Content-Type
              .AllowAnyMethod()); // incluye PATCH y DELETE
});

// ---------- Servicios ----------
builder.Services.AddControllers();
builder.Services.AddSingleton<TaskRepository>();
builder.Services.AddSingleton<UserRepository>();
builder.Services.AddSingleton<TokenService>();

var app = builder.Build();

// Si existe wwwroot (imagen Docker), se sirve el frontend Angular compilado.
app.UseDefaultFiles();
app.UseStaticFiles();

// El orden importa: CORS -> autenticación -> autorización -> controladores.
app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGet("/api/health", () => Results.Json(new { ok = true, message = "API de tareas (.NET + JWT) funcionando" }));

// Rutas de Angular (ej. /login, /tasks): devuelven index.html para que el router del cliente decida.
app.MapFallbackToFile("index.html");

app.Run();

// Necesario para que el proyecto de pruebas pueda arrancar la app (WebApplicationFactory).
public partial class Program { }
