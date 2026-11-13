using CitasMed.Api.Extensions;
using CitasMed.Api.Middleware;
using CitasMed.Api.Services;
using CitasMed.Application;
using CitasMed.Application.Common.Interfaces;
using CitasMed.Infrastructure;
using CitasMed.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// ---------- Servicios por capa ----------
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// ---------- Seguridad (autenticación JWT + autorización) ----------
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();

// ---------- Api ----------
builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddHealthChecks();
builder.Services.AddSwaggerWithJwt();
builder.Services.AddFrontendCors(builder.Configuration);

var app = builder.Build();

// ---------- Pipeline HTTP ----------
app.UseExceptionHandler();
app.UseStatusCodePages(); // 401/403/404 sin cuerpo también salen como ProblemDetails

// En Development todo viene activado; en Docker/producción se decide por variables de entorno.
var isDev = app.Environment.IsDevelopment();

if (isDev || app.Configuration.GetValue<bool>("Swagger:Enabled"))
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (isDev || app.Configuration.GetValue<bool>("Database:MigrateOnStartup"))
{
    var seed = isDev || app.Configuration.GetValue<bool>("Database:SeedDemoData");
    await app.Services.InitializeDatabaseAsync(seed); // migraciones (+ datos de demostración)
}

// CORS va ANTES de autenticación: las peticiones preflight (OPTIONS) no llevan token.
app.UseCors(CorsExtensions.FrontendPolicy);
app.UseAuthentication(); // ¿quién eres?  → lee y valida el JWT
app.UseAuthorization();  // ¿qué puedes hacer? → [Authorize], roles

app.MapControllers();
app.MapHealthChecks("/health").AllowAnonymous();

app.Run();

// Permite usar WebApplicationFactory<Program> en las pruebas de integración (fase 5).
public partial class Program;
