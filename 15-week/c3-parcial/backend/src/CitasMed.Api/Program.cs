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

// Fase 4: AddCors(...) con la política "Frontend"

var app = builder.Build();

// ---------- Pipeline HTTP ----------
app.UseExceptionHandler();
app.UseStatusCodePages(); // 401/403/404 sin cuerpo también salen como ProblemDetails

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    await app.Services.InitializeDatabaseAsync(); // migraciones + datos de prueba
}

// Fase 4: app.UseCors("Frontend");
app.UseAuthentication(); // ¿quién eres?  → lee y valida el JWT
app.UseAuthorization();  // ¿qué puedes hacer? → [Authorize], roles

app.MapControllers();
app.MapHealthChecks("/health").AllowAnonymous();

app.Run();

// Permite usar WebApplicationFactory<Program> en las pruebas de integración (fase 5).
public partial class Program;
