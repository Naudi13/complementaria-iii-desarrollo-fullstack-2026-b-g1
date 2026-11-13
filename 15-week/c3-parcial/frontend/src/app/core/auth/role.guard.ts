import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { Rol } from './auth.models';
import { AuthService } from './auth.service';

/**
 * Autorización en la UI: solo deja entrar a los roles indicados; si no, lleva al inicio del usuario.
 * Es una ayuda de navegación: el backend vuelve a validar el rol en cada endpoint (403).
 *
 * Uso: canActivate: [roleGuard('Admin')]
 */
export const roleGuard =
  (...roles: Rol[]): CanActivateFn =>
  () => {
    const auth = inject(AuthService);
    return auth.hasRole(...roles) ? true : inject(Router).parseUrl(auth.homeUrl());
  };
