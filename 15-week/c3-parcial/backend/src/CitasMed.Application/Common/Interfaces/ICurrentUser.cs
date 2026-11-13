namespace CitasMed.Application.Common.Interfaces;

/// <summary>
/// Usuario autenticado de la petición actual, leído de los claims del JWT.
/// Permite a los casos de uso aplicar autorización por recurso
/// (p. ej. "solo el paciente dueño puede cancelar su cita").
/// </summary>
public interface ICurrentUser
{
    bool IsAuthenticated { get; }

    /// <summary>Id del usuario (claim "sub"). Null si no está autenticado.</summary>
    int? Id { get; }

    /// <summary>Rol del usuario (claim "role").</summary>
    string? Rol { get; }
}
