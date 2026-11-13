using CitasMed.Domain.Entities;

namespace CitasMed.Application.Features.Especialidades;

public record EspecialidadDto(int Id, string Nombre)
{
    public static EspecialidadDto From(Especialidad e) => new(e.Id, e.Nombre);
}

public interface IEspecialidadRepository
{
    Task<IReadOnlyList<Especialidad>> GetAllAsync(CancellationToken ct = default);
    Task<Especialidad?> GetByIdAsync(int id, CancellationToken ct = default);
}

public interface IEspecialidadService
{
    Task<IReadOnlyList<EspecialidadDto>> GetAllAsync(CancellationToken ct = default);
}

public class EspecialidadService(IEspecialidadRepository repository) : IEspecialidadService
{
    public async Task<IReadOnlyList<EspecialidadDto>> GetAllAsync(CancellationToken ct = default) =>
        (await repository.GetAllAsync(ct)).Select(EspecialidadDto.From).ToList();
}
