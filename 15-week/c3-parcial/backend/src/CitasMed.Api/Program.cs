using CitasMed.Api.Middleware;
using CitasMed.Application;
using CitasMed.Infrastructure;
using CitasMed.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// ---------- Servicios por capa ----------
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// ---------- Api ----------
builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddHealthChecks();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Fase 2: AddAuthentication().AddJwtBearer(...) y AddAuthorization()
// Fase 4: AddCors(...) con la política "Frontend"

var app = builder.Build();

// ---------- Pipeline HTTP ----------
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    await app.Services.InitializeDatabaseAsync(); // migraciones + datos de prueba
}

// Fase 4: app.UseCors("Frontend");
// Fase 2: app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

// Permite usar WebApplicationFactory<Program> en las pruebas de integración (fase 5).
public partial class Program;
