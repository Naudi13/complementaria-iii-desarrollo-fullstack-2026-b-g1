// AuthAndTasksTests.cs
// Pruebas de integración: levantan la API real en memoria (WebApplicationFactory)
// y la llaman por HTTP, igual que lo haría el frontend.
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Backend.Tests;

public class AuthAndTasksTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public AuthAndTasksTests(WebApplicationFactory<Program> factory) => _factory = factory;

    // Helper: hace login con el usuario demo y devuelve un cliente ya autenticado.
    private async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/login",
            new { username = "admin", password = "Admin123!" });
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        var token = body.GetProperty("token").GetString();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsToken()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login",
            new { username = "admin", password = "Admin123!" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.False(string.IsNullOrWhiteSpace(body.GetProperty("token").GetString()));
    }

    // CASO DE ERROR: credenciales incorrectas -> 401 y sin token.
    [Fact]
    public async Task Login_WithWrongPassword_Returns401()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login",
            new { username = "admin", password = "incorrecta" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        var text = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("token", text, StringComparison.OrdinalIgnoreCase);
    }

    // CASO DE ERROR: endpoint protegido sin token -> 401.
    [Fact]
    public async Task GetTasks_WithoutToken_Returns401()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/tasks");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // CASO DE ERROR: token falso/manipulado -> 401.
    [Fact]
    public async Task GetTasks_WithInvalidToken_Returns401()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", "esto.no.esunjwt");

        var response = await client.GetAsync("/api/tasks");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetTasks_WithValidToken_ReturnsList()
    {
        var client = await CreateAuthenticatedClientAsync();

        var response = await client.GetAsync("/api/tasks");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(body.GetProperty("data").GetArrayLength() > 0);
    }

    [Fact]
    public async Task CreateTask_WithEmptyTitle_Returns400()
    {
        var client = await CreateAuthenticatedClientAsync();

        var response = await client.PostAsJsonAsync("/api/tasks", new { title = "  " });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateTask_WithValidTitle_Returns201()
    {
        var client = await CreateAuthenticatedClientAsync();

        var response = await client.PostAsJsonAsync("/api/tasks", new { title = "Nueva tarea de prueba" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    // CORS: una petición preflight desde el origen del frontend debe ser aceptada.
    [Fact]
    public async Task Cors_Preflight_FromAllowedOrigin_IsAllowed()
    {
        var client = _factory.CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Options, "/api/tasks");
        request.Headers.Add("Origin", "http://localhost:4200");
        request.Headers.Add("Access-Control-Request-Method", "GET");
        request.Headers.Add("Access-Control-Request-Headers", "authorization");

        var response = await client.SendAsync(request);

        Assert.True(response.Headers.Contains("Access-Control-Allow-Origin"));
        Assert.Equal("http://localhost:4200",
            response.Headers.GetValues("Access-Control-Allow-Origin").Single());
    }
}
