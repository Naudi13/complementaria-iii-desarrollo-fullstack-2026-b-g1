using CitasMed.Domain.Common;

namespace CitasMed.Domain.Entities;

public class Especialidad : BaseEntity
{
    public string Nombre { get; set; } = string.Empty;

    public ICollection<Medico> Medicos { get; set; } = new List<Medico>();
}
