using CitasMed.Application.Common.Exceptions;
using CitasMed.Application.Common.Interfaces;
using CitasMed.Application.Features.Especialidades;
using CitasMed.Application.Features.Usuarios;
using CitasMed.Domain.Entities;
using CitasMed.Domain.Enums;

namespace CitasMed.Application.Features.Medicos;

public interface IMedicoService
{
    Task<IReadOnlyList<MedicoDto>> GetAllAsync(int? especialidadId, CancellationToken ct = default);
    Task<MedicoDto> GetByIdAsync(int id, CancellationToken ct = default);
    Task<MedicoDto> CrearAsync(CrearMedicoRequest request, CancellationToken ct = default);
    Task EliminarAsync(int id, CancellationToken ct = default);
}

public class MedicoService(
    IMedicoRepository medicos,
    IEspecialidadRepository especialidades,
    IUserRepository users,
    IPasswordHasher hasher,
    IUnitOfWork unitOfWork) : IMedicoService
{
    public async Task<IReadOnlyList<MedicoDto>> GetAllAsync(int? especialidadId, CancellationToken ct = default) =>
        (await medicos.GetAllAsync(especialidadId, ct)).Select(MedicoDto.From).ToList();

    public async Task<MedicoDto> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var medico = await medicos.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"No existe el médico {id}.");
        return MedicoDto.From(medico);
    }

    public async Task<MedicoDto> CrearAsync(CrearMedicoRequest request, CancellationToken ct = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var registro = request.RegistroProfesional.Trim().ToUpperInvariant();

        var especialidad = await especialidades.GetByIdAsync(request.EspecialidadId, ct)
            ?? throw new NotFoundException($"No existe la especialidad {request.EspecialidadId}.");

        if (await users.ExistsByEmailAsync(email, ct))
            throw new ConflictException("Ya existe un usuario con ese email.");

        if (await medicos.ExistsRegistroAsync(registro, ct))
            throw new ConflictException("Ya existe un médico con ese registro profesional.");

        var medico = new Medico
        {
            RegistroProfesional = registro,
            EspecialidadId = especialidad.Id,
            Especialidad = especialidad,
            User = new User
            {
                Nombre = request.Nombre.Trim(),
                Email = email,
                PasswordHash = hasher.Hash(request.Password),
                Rol = Rol.Medico
            }
        };

        medicos.Add(medico);
        await unitOfWork.SaveChangesAsync(ct);

        return MedicoDto.From(medico);
    }

    public async Task EliminarAsync(int id, CancellationToken ct = default)
    {
        var medico = await medicos.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"No existe el médico {id}.");

        if (await medicos.TieneCitasAsync(id, ct))
            throw new ConflictException("No se puede eliminar un médico con citas registradas.");

        medicos.Remove(medico);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
