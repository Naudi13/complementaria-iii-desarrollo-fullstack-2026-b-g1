using CitasMed.Domain.Common;

namespace CitasMed.Domain.Entities;

public class Medico : BaseEntity
{
    public string RegistroProfesional { get; set; } = string.Empty;

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int EspecialidadId { get; set; }
    public Especialidad Especialidad { get; set; } = null!;

    public ICollection<Cita> Citas { get; set; } = new List<Cita>();
}
