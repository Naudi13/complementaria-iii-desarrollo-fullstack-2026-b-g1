namespace CitasMed.Application.Common.Interfaces;

/// <summary>
/// Confirma en una sola transacción los cambios hechos a través de los repositorios.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
