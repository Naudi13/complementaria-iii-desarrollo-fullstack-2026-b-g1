namespace CitasMed.Application.Common.Exceptions;

// Excepciones de aplicación. El GlobalExceptionHandler de la Api
// las traduce al código HTTP correspondiente (ProblemDetails).

/// <summary>404 Not Found.</summary>
public class NotFoundException(string message) : Exception(message);

/// <summary>401 Unauthorized: credenciales inválidas.</summary>
public class UnauthorizedException(string message) : Exception(message);

/// <summary>403 Forbidden: autenticado, pero sin permiso sobre el recurso.</summary>
public class ForbiddenException(string message) : Exception(message);

/// <summary>409 Conflict: p. ej. horario ya ocupado o email repetido.</summary>
public class ConflictException(string message) : Exception(message);
