using CitasMed.Domain.Entities;

namespace CitasMed.Application.Features.Medicos;

public interface IMedicoRepository
{
    /// <summary>Lista médicos (con User y Especialidad), opcionalmente filtrados por especialidad.</summary>
    Task<IReadOnlyList<Medico>> GetAllAsync(int? especialidadId = null, CancellationToken ct = default);

    /// <summary>Obtiene un médico (con User y Especialidad) para lectura o modificación.</summary>
    Task<Medico?> GetByIdAsync(int id, CancellationToken ct = default);

    /// <summary>Perfil de médico asociado a un usuario (para saber "qué médico soy" desde el token).</summary>
    Task<Medico?> GetByUserIdAsync(int userId, CancellationToken ct = default);

    Task<bool> ExistsAsync(int id, CancellationToken ct = default);
    Task<bool> ExistsRegistroAsync(string registroProfesional, CancellationToken ct = default);
    Task<bool> TieneCitasAsync(int medicoId, CancellationToken ct = default);

    void Add(Medico medico);

    /// <summary>Elimina el médico y su usuario asociado.</summary>
    void Remove(Medico medico);
}
