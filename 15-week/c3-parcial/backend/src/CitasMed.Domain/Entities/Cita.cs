using CitasMed.Domain.Common;
using CitasMed.Domain.Enums;
using CitasMed.Domain.Exceptions;

namespace CitasMed.Domain.Entities;

public class Cita : BaseEntity
{
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
