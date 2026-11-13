using CitasMed.Domain.Common;
using CitasMed.Domain.Enums;
using CitasMed.Domain.Exceptions;

namespace CitasMed.Domain.Entities;

public class Cita : BaseEntity
{
    public const int DuracionMinutos = 30;
    public const int HoraApertura = 7;   // 07:00
    public const int HoraCierre = 18;    // última cita 17:30

    /// <summary>
    /// Crea una cita validando las reglas del dominio:
    /// debe ser futura, en franjas de 30 minutos y dentro del horario de atención.
    /// </summary>
    public static Cita Programar(int pacienteId, int medicoId, DateTime fechaHora, string motivo, DateTime ahora)
    {
        if (fechaHora <= ahora)
            throw new DomainException("La cita debe programarse en una fecha futura.");

        if (fechaHora.Minute % DuracionMinutos != 0 || fechaHora.Second != 0 || fechaHora.Millisecond != 0)
            throw new DomainException("Las citas se asignan en franjas de 30 minutos (ej. 08:00, 08:30).");

        if (fechaHora.Hour < HoraApertura || fechaHora.Hour >= HoraCierre)
            throw new DomainException($"El horario de atención es de {HoraApertura:00}:00 a {HoraCierre:00}:00.");

        if (string.IsNullOrWhiteSpace(motivo))
            throw new DomainException("El motivo de la cita es obligatorio.");

        return new Cita
        {
            PacienteId = pacienteId,
            MedicoId = medicoId,
            FechaHora = fechaHora,
            Motivo = motivo.Trim()
        };
    }

    public DateTime FechaHora { get; set; }
    public string Motivo { get; set; } = string.Empty;
    public EstadoCita Estado { get; private set; } = EstadoCita.Pendiente;

    public int PacienteId { get; set; }
    public User Paciente { get; set; } = null!;

    public int MedicoId { get; set; }
    public Medico Medico { get; set; } = null!;

    /// <summary>Indica si la cita pertenece al paciente dado (autorización por recurso).</summary>
    public bool PerteneceA(int pacienteId) => PacienteId == pacienteId;

    public void Cancelar()
    {
        if (Estado != EstadoCita.Pendiente)
            throw new DomainException("Solo se pueden cancelar citas pendientes.");

        Estado = EstadoCita.Cancelada;
    }

    public void Atender()
    {
        if (Estado != EstadoCita.Pendiente)
            throw new DomainException("Solo se pueden atender citas pendientes.");

        Estado = EstadoCita.Atendida;
    }
}
