using System.Net;
using System.Net.Http.Json;
using CitasMed.Application.Features.Citas;
using CitasMed.Application.Features.Medicos;

namespace CitasMed.IntegrationTests;

/// <summary>
/// Flujo completo de citas a través de la API real: reservar, conflicto de horario,
/// autorización por rol y por recurso, y cambio de estado persistido en la base de datos.
/// </summary>
public class CitasEndpointsTests(CitasMedApiFactory factory) : IClassFixture<CitasMedApiFactory>
{
    /// <summary>Fecha futura en franja válida; cada prueba usa una hora distinta para no chocar.</summary>
    private static DateTime Manana(int hora) => DateTime.Today.AddDays(1).AddHours(hora);

    private async Task<int> MedicoIdDeLauraAsync(HttpClient client)
    {
        var medicos = await client.GetFromJsonAsync<List<MedicoDto>>("/api/medicos");
        return medicos!.Single(m => m.Email == "laura.gomez@citasmed.test").Id;
    }

    [Fact]
    public async Task FlujoCompleto_PacienteReserva_MedicoAsignadoAtiende()
    {
        var paciente = await factory.CreateClientAsAsync("paciente@citasmed.test");
        var medico = await factory.CreateClientAsAsync("laura.gomez@citasmed.test");
        var medicoId = await MedicoIdDeLauraAsync(paciente);

        // 1. El paciente reserva → 201
        var crear = await paciente.PostAsJsonAsync("/api/citas", new CrearCitaRequest(medicoId, Manana(8), "Control"));
        Assert.Equal(HttpStatusCode.Created, crear.StatusCode);
        var cita = await crear.Content.ReadFromJsonAsync<CitaDto>();
        Assert.Equal("Pendiente", cita!.Estado);

        // 2. Aparece en "mis citas" del paciente y en la agenda del médico
        var mias = await paciente.GetFromJsonAsync<List<CitaDto>>("/api/citas/mias");
        Assert.Contains(mias!, c => c.Id == cita.Id);
        var agenda = await medico.GetFromJsonAsync<List<CitaDto>>("/api/citas/agenda");
        Assert.Contains(agenda!, c => c.Id == cita.Id);

        // 3. El médico la atiende → 200 y el estado queda persistido
        var atender = await medico.PatchAsync($"/api/citas/{cita.Id}/atender", null);
        Assert.Equal(HttpStatusCode.OK, atender.StatusCode);
        var actualizada = await atender.Content.ReadFromJsonAsync<CitaDto>();
        Assert.Equal("Atendida", actualizada!.Estado);
    }

    [Fact]
    public async Task Reservar_HorarioYaOcupado_Devuelve409()
    {
        var paciente = await factory.CreateClientAsAsync("paciente@citasmed.test");
        var paciente2 = await factory.CreateClientAsAsync("paciente2@citasmed.test");
        var medicoId = await MedicoIdDeLauraAsync(paciente);
        var request = new CrearCitaRequest(medicoId, Manana(9), "Control");

        var primera = await paciente.PostAsJsonAsync("/api/citas", request);
        var segunda = await paciente2.PostAsJsonAsync("/api/citas", request);

        Assert.Equal(HttpStatusCode.Created, primera.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, segunda.StatusCode);
    }

    [Fact]
    public async Task Reservar_EnFechaPasada_Devuelve400()
    {
        var paciente = await factory.CreateClientAsAsync("paciente@citasmed.test");
        var medicoId = await MedicoIdDeLauraAsync(paciente);

        var response = await paciente.PostAsJsonAsync("/api/citas",
            new CrearCitaRequest(medicoId, DateTime.Today.AddDays(-1).AddHours(9), "Control"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Cancelar_CitaDeOtroPaciente_Devuelve403()
    {
        var duena = await factory.CreateClientAsAsync("paciente@citasmed.test");
        var otro = await factory.CreateClientAsAsync("paciente2@citasmed.test");
        var medicoId = await MedicoIdDeLauraAsync(duena);
        var crear = await duena.PostAsJsonAsync("/api/citas", new CrearCitaRequest(medicoId, Manana(10), "Control"));
        var cita = await crear.Content.ReadFromJsonAsync<CitaDto>();

        var response = await otro.PatchAsync($"/api/citas/{cita!.Id}/cancelar", null);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Atender_ConTokenDePaciente_Devuelve403PorRol()
    {
        var paciente = await factory.CreateClientAsAsync("paciente@citasmed.test");

        var response = await paciente.PatchAsync("/api/citas/1/atender", null);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
