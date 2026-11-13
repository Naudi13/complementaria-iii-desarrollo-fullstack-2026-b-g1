import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from './auth.service';

/** Autenticación en la UI: sin sesión válida → /login (recordando a dónde quería ir). */
export const authGuard: CanActivateFn = (_route, state) => {
  const auth = inject(AuthService);
  const router = inject(Router);

  return auth.hasValidSession()
    ? true
    : router.createUrlTree(['/login'], { queryParams: { returnUrl: state.url } });
};
