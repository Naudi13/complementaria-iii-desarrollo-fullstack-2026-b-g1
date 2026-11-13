import { TestBed } from '@angular/core/testing';
import { ActivatedRouteSnapshot, Router, RouterStateSnapshot, UrlTree, provideRouter } from '@angular/router';
import { authGuard } from './auth.guard';
import { AuthService } from './auth.service';
import { roleGuard } from './role.guard';
import { Rol } from './auth.models';

describe('guards de navegación', () => {
  const authStub = {
    valid: false,
    rol: null as Rol | null,
    hasValidSession() {
      return this.valid;
    },
    hasRole(...roles: Rol[]) {
      return this.rol !== null && roles.includes(this.rol);
    },
    homeUrl() {
      return '/citas/mias';
    },
  };

  const route = {} as ActivatedRouteSnapshot;
  const state = { url: '/citas/reservar' } as RouterStateSnapshot;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideRouter([]), { provide: AuthService, useValue: authStub }],
    });
  });

  const run = (guard: typeof authGuard) => TestBed.runInInjectionContext(() => guard(route, state));
  const url = (result: unknown) => TestBed.inject(Router).serializeUrl(result as UrlTree);

  it('authGuard: sin sesión redirige a /login con returnUrl', () => {
    authStub.valid = false;

    const result = run(authGuard);

    expect(url(result)).toBe('/login?returnUrl=%2Fcitas%2Freservar');
  });

  it('authGuard: con sesión válida permite la navegación', () => {
    authStub.valid = true;

    expect(run(authGuard)).toBe(true);
  });

  it('roleGuard: permite el acceso al rol indicado', () => {
    authStub.rol = 'Admin';

    expect(run(roleGuard('Admin'))).toBe(true);
  });

  it('roleGuard: otro rol es redirigido a su página de inicio', () => {
    authStub.rol = 'Paciente';

    const result = run(roleGuard('Admin'));

    expect(url(result)).toBe('/citas/mias');
  });
});
