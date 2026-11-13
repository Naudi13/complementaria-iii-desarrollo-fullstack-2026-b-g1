using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using CitasMed.Application.Features.Auth;
using CitasMed.Infrastructure.Persistence;

namespace CitasMed.IntegrationTests;

/// <summary>
/// Pruebas de integración de seguridad: autenticación (401) y autorización por rol (403)
/// atravesando el pipeline HTTP real.
/// </summary>
public class AuthEndpointsTests(CitasMedApiFactory factory) : IClassFixture<CitasMedApiFactory>
{
    [Fact]
    public async Task Login_ConCredencialesValidas_Devuelve200YToken()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest("paciente@citasmed.test", DbSeeder.DevPassword));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.False(string.IsNullOrWhiteSpace(body!.AccessToken));
        Assert.Equal(3, body.AccessToken.Split('.').Length); // header.payload.firma
        Assert.Equal("Paciente", body.Usuario.Rol);
    }

    [Fact]
    public async Task Login_ConPasswordIncorrecta_Devuelve401()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest("paciente@citasmed.test", "incorrecta"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [InlineData("/api/citas/mias")]
    [InlineData("/api/medicos")]
    [InlineData("/api/auth/me")]
    public async Task EndpointProtegido_SinToken_Devuelve401(string url)
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync(url);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task EndpointProtegido_ConTokenManipulado_Devuelve401()
    {
        var client = await factory.CreateClientAsAsync("admin@citasmed.test");
        var token = client.DefaultRequestHeaders.Authorization!.Parameter!;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token[..^2] + "xx");

        var response = await client.GetAsync("/api/usuarios");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task EndpointDeAdmin_ConTokenDePaciente_Devuelve403()
    {
        var client = await factory.CreateClientAsAsync("paciente@citasmed.test");

        var response = await client.GetAsync("/api/usuarios");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task EndpointDeAdmin_ConTokenDeAdmin_Devuelve200()
    {
        var client = await factory.CreateClientAsAsync("admin@citasmed.test");

        var response = await client.GetAsync("/api/usuarios");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Cors_PreflightDesdeOrigenPermitido_IncluyeCabeceraAllowOrigin()
    {
        var client = factory.CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Options, "/api/citas/mias");
        request.Headers.Add("Origin", "http://localhost:4200");
        request.Headers.Add("Access-Control-Request-Method", "GET");
        request.Headers.Add("Access-Control-Request-Headers", "authorization");

        var response = await client.SendAsync(request);

        Assert.True(response.Headers.TryGetValues("Access-Control-Allow-Origin", out var values));
        Assert.Equal("http://localhost:4200", values.Single());
    }

    [Fact]
    public async Task Cors_PreflightDesdeOrigenNoPermitido_NoIncluyeCabeceraAllowOrigin()
    {
        var client = factory.CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Options, "/api/citas/mias");
        request.Headers.Add("Origin", "http://sitio-malicioso.test");
        request.Headers.Add("Access-Control-Request-Method", "GET");

        var response = await client.SendAsync(request);

        Assert.False(response.Headers.Contains("Access-Control-Allow-Origin"));
    }
}
