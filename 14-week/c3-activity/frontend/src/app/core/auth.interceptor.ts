// core/auth.interceptor.ts
// Interceptor HTTP: añade "Authorization: Bearer <token>" a cada petición hacia la API
// y, si el servidor responde 401 (token vencido o inválido), cierra la sesión y manda al login.
import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { API_URL } from '../config';
import { AuthService } from './auth.service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);
  const router = inject(Router);

  // El token solo se envía a nuestra API, nunca a terceros.
  const token = auth.token();
  const authReq =
    token && req.url.startsWith(API_URL)
      ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
      : req;

  return next(authReq).pipe(
    catchError((error: HttpErrorResponse) => {
      // 401 en una ruta que no es el login = sesión expirada.
      if (error.status === 401 && !req.url.endsWith('/auth/login')) {
        auth.logout();
        router.navigate(['/login']);
      }
      return throwError(() => error);
    }),
  );
};
