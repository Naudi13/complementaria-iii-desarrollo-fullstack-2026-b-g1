import { HttpErrorResponse } from '@angular/common/http';

/** Forma de las respuestas de error de la API (ProblemDetails, RFC 9457). */
interface ProblemDetails {
  title?: string;
  detail?: string;
  status?: number;
  errors?: Record<string, string[]>;
}

/**
 * Convierte cualquier error HTTP en un mensaje legible para el usuario.
 */
export function getErrorMessage(error: unknown): string {
  if (!(error instanceof HttpErrorResponse)) {
    return 'Ocurrió un error inesperado.';
  }

  if (error.status === 0) {
    return 'No se pudo conectar con el servidor. Verifica que la API esté en ejecución.';
  }

  const problem = (error.error ?? {}) as ProblemDetails;

  if (problem.errors) {
    return Object.values(problem.errors).flat().join(' ');
  }

  if (problem.detail) {
    return problem.detail;
  }

  switch (error.status) {
    case 403:
      return 'No tienes permisos para realizar esta acción.';
    case 404:
      return 'El recurso solicitado no existe.';
    default:
      return problem.title ?? 'Error del servidor. Intenta de nuevo más tarde.';
  }
}
