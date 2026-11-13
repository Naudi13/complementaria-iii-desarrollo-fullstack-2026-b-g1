using System.Net.Http.Headers;
using System.Net.Http.Json;
using CitasMed.Application.Features.Auth;
using CitasMed.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;

namespace CitasMed.IntegrationTests;

/// <summary>
/// Levanta la API real en memoria (pipeline completo: routing, CORS, JWT, autorización,
/// controladores, servicios, EF Core) contra una base SQLite temporal y aislada.
/// Al arrancar en "Development" se aplican las migraciones y el seed de prueba.
/// </summary>
public class CitasMedApiFactory : WebApplicationFactory<Program>
{
    private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"citasmed-tests-{Guid.NewGuid():N}.db");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.UseSetting("ConnectionStrings:Default", $"Data Source={_dbPath}");
        builder.UseSetting("Cors:AllowedOrigins", "http://localhost:4200");
    }

    /// <summary>Hace login con un usuario del seed y devuelve un cliente con el Bearer token.</summary>
    public async Task<HttpClient> CreateClientAsAsync(string email)
    {
        var client = CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, DbSeeder.DevPassword));
        response.EnsureSuccessStatusCode();

        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth!.AccessToken);
        return client;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        SqliteConnection.ClearAllPools(); // libera el archivo para poder borrarlo
        try { File.Delete(_dbPath); } catch (IOException) { /* se limpia con el temp del sistema */ }
    }
}
