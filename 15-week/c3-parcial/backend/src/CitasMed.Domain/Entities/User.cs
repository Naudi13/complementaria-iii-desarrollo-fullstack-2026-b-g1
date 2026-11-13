using CitasMed.Domain.Common;
using CitasMed.Domain.Enums;

namespace CitasMed.Domain.Entities;

public class User : BaseEntity
{
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    /// <summary>Hash BCrypt. Nunca se guarda la contraseña en texto plano.</summary>
    public string PasswordHash { get; set; } = string.Empty;

    public Rol Rol { get; set; }

    /// <summary>Perfil de médico (solo si Rol == Medico).</summary>
    public Medico? Medico { get; set; }

    /// <summary>Citas solicitadas (solo si Rol == Paciente).</summary>
    public ICollection<Cita> CitasComoPaciente { get; set; } = new List<Cita>();
}
