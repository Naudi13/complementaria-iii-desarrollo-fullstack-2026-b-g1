using System.ComponentModel.DataAnnotations;
using CitasMed.Domain.Entities;

namespace CitasMed.Application.Features.Citas;

public record CitaDto(
    int Id,
    DateTime FechaHora,
    string Motivo,
    string Estado,
    int MedicoId,
    string Medico,
    string Especialidad,
    int PacienteId,
    string Paciente)
{
    /// <summary>Requiere Paciente, Medico.User y Medico.Especialidad cargados.</summary>
    public static CitaDto From(Cita c) => new(
        c.Id, c.FechaHora, c.Motivo, c.Estado.ToString(),
        c.MedicoId, c.Medico.User.Nombre, c.Medico.Especialidad.Nombre,
        c.PacienteId, c.Paciente.Nombre);
}

public record CrearCitaRequest(
    [Range(1, int.MaxValue, ErrorMessage = "Seleccione un médico.")] int MedicoId,
    [Required] DateTime FechaHora,
    [Required(ErrorMessage = "El motivo es obligatorio."), StringLength(300)] string Motivo);
