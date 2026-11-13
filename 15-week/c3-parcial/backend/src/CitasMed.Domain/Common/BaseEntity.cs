namespace CitasMed.Domain.Common;

/// <summary>
/// Clase base para todas las entidades: identificador y fecha de creación.
/// </summary>
public abstract class BaseEntity
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
