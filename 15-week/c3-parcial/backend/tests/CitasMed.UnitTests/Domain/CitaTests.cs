using CitasMed.Domain.Entities;
using CitasMed.Domain.Enums;
using CitasMed.Domain.Exceptions;

namespace CitasMed.UnitTests.Domain;

/// <summary>
/// Pruebas unitarias de las reglas de negocio de la entidad Cita.
/// No usan base de datos ni mocks: el dominio es lógica pura.
/// </summary>
public class CitaTests
{
    private static readonly DateTime Ahora = new(2026, 11, 13, 10, 0, 0);
    private static readonly DateTime Manana9am = new(2026, 11, 14, 9, 0, 0);

    [Fact]
    public void Programar_ConDatosValidos_CreaCitaPendiente()
    {
        var cita = Cita.Programar(pacienteId: 1, medicoId: 2, Manana9am, "  Control  ", Ahora);

        Assert.Equal(EstadoCita.Pendiente, cita.Estado);
        Assert.Equal(1, cita.PacienteId);
        Assert.Equal(2, cita.MedicoId);
        Assert.Equal("Control", cita.Motivo);
    }

    [Fact]
    public void Programar_EnFechaPasada_LanzaDomainException()
    {
        var ex = Assert.Throws<DomainException>(() =>
            Cita.Programar(1, 2, Ahora.AddHours(-1), "Control", Ahora));

        Assert.Contains("futura", ex.Message);
    }

    [Theory]
    [InlineData(9, 15)]
    [InlineData(9, 45)]
    [InlineData(10, 1)]
    public void Programar_FueraDeFranjaDe30Minutos_LanzaDomainException(int hora, int minuto)
    {
        var fecha = new DateTime(2026, 11, 14, hora, minuto, 0);

        Assert.Throws<DomainException>(() => Cita.Programar(1, 2, fecha, "Control", Ahora));
    }

    [Theory]
    [InlineData(6, 30)]
    [InlineData(18, 0)]
    [InlineData(20, 0)]
    public void Programar_FueraDelHorarioDeAtencion_LanzaDomainException(int hora, int minuto)
    {
        var fecha = new DateTime(2026, 11, 14, hora, minuto, 0);

        Assert.Throws<DomainException>(() => Cita.Programar(1, 2, fecha, "Control", Ahora));
    }

    [Fact]
    public void Atender_CitaPendiente_CambiaEstadoAAtendida()
    {
        var cita = Cita.Programar(1, 2, Manana9am, "Control", Ahora);

        cita.Atender();

        Assert.Equal(EstadoCita.Atendida, cita.Estado);
    }

    [Fact]
    public void Cancelar_CitaYaAtendida_LanzaDomainException()
    {
        var cita = Cita.Programar(1, 2, Manana9am, "Control", Ahora);
        cita.Atender();

        Assert.Throws<DomainException>(() => cita.Cancelar());
        Assert.Equal(EstadoCita.Atendida, cita.Estado);
    }

    [Fact]
    public void PerteneceA_IndicaSiLaCitaEsDelPaciente()
    {
        var cita = Cita.Programar(pacienteId: 7, medicoId: 2, Manana9am, "Control", Ahora);

        Assert.True(cita.PerteneceA(7));
        Assert.False(cita.PerteneceA(8));
    }
}
