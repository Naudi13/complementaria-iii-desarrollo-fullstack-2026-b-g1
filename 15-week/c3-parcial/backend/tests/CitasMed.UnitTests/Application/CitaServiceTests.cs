using CitasMed.Application.Common.Exceptions;
using CitasMed.Application.Common.Interfaces;
using CitasMed.Application.Features.Citas;
using CitasMed.Application.Features.Medicos;
using CitasMed.Domain.Entities;
using CitasMed.Domain.Enums;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace CitasMed.UnitTests.Application;

/// <summary>
/// Pruebas unitarias de los casos de uso de citas.
/// Se verifica la lógica de negocio y la autorización por recurso,
/// sin base de datos (repositorios simulados) y con un reloj controlado.
/// </summary>
public class CitaServiceTests
{
    private const int PacienteId = 10;
    private const int OtroPacienteId = 11;
    private const int MedicoId = 1;
    private const int MedicoUserId = 20;

    private readonly Mock<ICitaRepository> _citas = new();
    private readonly Mock<IMedicoRepository> _medicos = new();
    private readonly Mock<ICurrentUser> _currentUser = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly FakeTimeProvider _clock = new();

    private readonly DateTime _manana9am;

    public CitaServiceTests()
    {
        _clock.SetLocalTimeZone(TimeZoneInfo.Utc);
        _clock.SetUtcNow(new DateTimeOffset(2026, 11, 13, 10, 0, 0, TimeSpan.Zero));
        _manana9am = new DateTime(2026, 11, 14, 9, 0, 0);

        _medicos.Setup(m => m.ExistsAsync(MedicoId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
    }

    private CitaService CrearServicio() =>
        new(_citas.Object, _medicos.Object, _currentUser.Object, _uow.Object, _clock);

    private void ComoUsuario(int id, Rol rol)
    {
        _currentUser.Setup(u => u.Id).Returns(id);
        _currentUser.Setup(u => u.Rol).Returns(rol.ToString());
    }

    /// <summary>Cita con sus relaciones cargadas, como la devolvería el repositorio.</summary>
    private Cita CitaDe(int pacienteId, int medicoId = MedicoId)
    {
        var cita = Cita.Programar(pacienteId, medicoId, _manana9am, "Control", _clock.GetLocalNow().DateTime);
        cita.Id = 99;
        cita.Paciente = new User { Id = pacienteId, Nombre = "Paciente" };
        cita.Medico = new Medico
        {
            Id = medicoId,
            UserId = MedicoUserId,
            User = new User { Nombre = "Dra. Prueba" },
            Especialidad = new Especialidad { Nombre = "General" }
        };
        return cita;
    }

    [Fact]
    public async Task CrearAsync_EnHorarioLibre_GuardaLaCita()
    {
        ComoUsuario(PacienteId, Rol.Paciente);
        _citas.Setup(c => c.HorarioOcupadoAsync(MedicoId, _manana9am, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _citas.Setup(c => c.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(CitaDe(PacienteId));

        var dto = await CrearServicio().CrearAsync(new CrearCitaRequest(MedicoId, _manana9am, "Control"));

        Assert.Equal("Pendiente", dto.Estado);
        _citas.Verify(c => c.Add(It.Is<Cita>(x => x.PacienteId == PacienteId && x.MedicoId == MedicoId)), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CrearAsync_EnHorarioOcupado_LanzaConflictYNoGuarda()
    {
        ComoUsuario(PacienteId, Rol.Paciente);
        _citas.Setup(c => c.HorarioOcupadoAsync(MedicoId, _manana9am, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        await Assert.ThrowsAsync<ConflictException>(() =>
            CrearServicio().CrearAsync(new CrearCitaRequest(MedicoId, _manana9am, "Control")));

        _citas.Verify(c => c.Add(It.IsAny<Cita>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CrearAsync_ConMedicoInexistente_LanzaNotFound()
    {
        ComoUsuario(PacienteId, Rol.Paciente);
        _medicos.Setup(m => m.ExistsAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            CrearServicio().CrearAsync(new CrearCitaRequest(999, _manana9am, "Control")));
    }

    [Fact]
    public async Task CancelarAsync_CitaDeOtroPaciente_LanzaForbidden()
    {
        ComoUsuario(OtroPacienteId, Rol.Paciente);
        var cita = CitaDe(PacienteId);
        _citas.Setup(c => c.GetByIdAsync(cita.Id, It.IsAny<CancellationToken>())).ReturnsAsync(cita);

        await Assert.ThrowsAsync<ForbiddenException>(() => CrearServicio().CancelarAsync(cita.Id));

        Assert.Equal(EstadoCita.Pendiente, cita.Estado);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CancelarAsync_CitaPropia_LaCancela()
    {
        ComoUsuario(PacienteId, Rol.Paciente);
        var cita = CitaDe(PacienteId);
        _citas.Setup(c => c.GetByIdAsync(cita.Id, It.IsAny<CancellationToken>())).ReturnsAsync(cita);

        var dto = await CrearServicio().CancelarAsync(cita.Id);

        Assert.Equal("Cancelada", dto.Estado);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CancelarAsync_ComoAdmin_PuedeCancelarCitaAjena()
    {
        ComoUsuario(id: 1, Rol.Admin);
        var cita = CitaDe(PacienteId);
        _citas.Setup(c => c.GetByIdAsync(cita.Id, It.IsAny<CancellationToken>())).ReturnsAsync(cita);

        var dto = await CrearServicio().CancelarAsync(cita.Id);

        Assert.Equal("Cancelada", dto.Estado);
    }

    [Fact]
    public async Task AtenderAsync_MedicoNoAsignado_LanzaForbidden()
    {
        const int otroMedicoUserId = 30;
        ComoUsuario(otroMedicoUserId, Rol.Medico);
        var cita = CitaDe(PacienteId, medicoId: MedicoId);
        _citas.Setup(c => c.GetByIdAsync(cita.Id, It.IsAny<CancellationToken>())).ReturnsAsync(cita);
        _medicos.Setup(m => m.GetByUserIdAsync(otroMedicoUserId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Medico { Id = 2, UserId = otroMedicoUserId });

        await Assert.ThrowsAsync<ForbiddenException>(() => CrearServicio().AtenderAsync(cita.Id));
        Assert.Equal(EstadoCita.Pendiente, cita.Estado);
    }
}
