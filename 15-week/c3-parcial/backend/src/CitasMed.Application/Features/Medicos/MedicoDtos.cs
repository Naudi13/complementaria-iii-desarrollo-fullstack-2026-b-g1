using System.ComponentModel.DataAnnotations;
using CitasMed.Domain.Entities;

namespace CitasMed.Application.Features.Medicos;

public record MedicoDto(int Id, string Nombre, string Email, string RegistroProfesional, int EspecialidadId, string Especialidad)
{
    /// <summary>Requiere que User y Especialidad estén cargados.</summary>
    public static MedicoDto From(Medico m) =>
        new(m.Id, m.User.Nombre, m.User.Email, m.RegistroProfesional, m.EspecialidadId, m.Especialidad.Nombre);
}

public record CrearMedicoRequest(
    [Required, StringLength(120, MinimumLength = 3)] string Nombre,
    [Required, EmailAddress, StringLength(150)] string Email,
    [Required, StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")] string Password,
    [Required, StringLength(30)] string RegistroProfesional,
    [Range(1, int.MaxValue, ErrorMessage = "Seleccione una especialidad.")] int EspecialidadId);
