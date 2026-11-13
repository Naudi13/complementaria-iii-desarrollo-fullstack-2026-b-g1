using CitasMed.Application.Common.Exceptions;
using CitasMed.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace CitasMed.Api.Middleware;

/// <summary>
/// Manejo global de errores: traduce excepciones a respuestas ProblemDetails (RFC 9457)
/// para que los controladores no necesiten try/catch y el frontend reciba un formato uniforme.
/// </summary>
public class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (status, title) = exception switch
        {
            NotFoundException     => (StatusCodes.Status404NotFound, "Recurso no encontrado"),
            UnauthorizedException => (StatusCodes.Status401Unauthorized, "No autenticado"),
            ForbiddenException    => (StatusCodes.Status403Forbidden, "Acceso denegado"),
            ConflictException     => (StatusCodes.Status409Conflict, "Conflicto"),
            DomainException       => (StatusCodes.Status400BadRequest, "Regla de negocio no cumplida"),
            _                     => (StatusCodes.Status500InternalServerError, "Error interno del servidor")
        };

        if (status == StatusCodes.Status500InternalServerError)
            logger.LogError(exception, "Error no controlado");

        httpContext.Response.StatusCode = status;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = status,
                Title = title,
                // En errores 500 no se expone el mensaje interno al cliente.
                Detail = status == StatusCodes.Status500InternalServerError ? null : exception.Message
            }
        });
    }
}
