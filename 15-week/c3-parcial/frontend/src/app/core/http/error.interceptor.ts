import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { AuthService } from '../auth/auth.service';

/**
 * Manejo global de errores HTTP:
 * - 401 en cualquier endpoint (salvo el login) → token inválido o vencido → cerrar sesión.
 * - El resto se re-lanza para que cada pantalla muestre su propio mensaje.
 */
export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);

  return next(req).pipe(
    catchError((error: unknown) => {
      const esLogin = req.url.endsWith('/auth/login');
      if (error instanceof HttpErrorResponse && error.status === 401 && !esLogin) {
        auth.logout(true);
      }
      return throwError(() => error);
    }),
  );
};
