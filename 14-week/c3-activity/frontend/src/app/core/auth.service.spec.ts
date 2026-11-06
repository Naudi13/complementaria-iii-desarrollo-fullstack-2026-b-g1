// Pruebas del servicio de autenticación (login correcto y caso de error).
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { API_URL } from '../config';
import { AuthService } from './auth.service';

describe('AuthService', () => {
  let service: AuthService;
  let http: HttpTestingController;

  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(AuthService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('guarda el token y marca la sesión como iniciada tras un login correcto', () => {
    service.login('admin', 'Admin123!').subscribe();

    const req = http.expectOne(`${API_URL}/auth/login`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ username: 'admin', password: 'Admin123!' });
    req.flush({ token: 'abc.def.ghi', expiresAt: '2030-01-01T00:00:00Z', username: 'admin' });

    expect(service.isLoggedIn()).toBe(true);
    expect(service.token()).toBe('abc.def.ghi');
    expect(localStorage.getItem('jwt_token')).toBe('abc.def.ghi');
  });

  // CASO DE ERROR: el servidor responde 401 -> no debe quedar ninguna sesión.
  it('no guarda token cuando las credenciales son incorrectas (401)', () => {
    let status = 0;
    service.login('admin', 'mala').subscribe({ error: (e) => (status = e.status) });

    http
      .expectOne(`${API_URL}/auth/login`)
      .flush({ ok: false, error: 'Credenciales inválidas' }, { status: 401, statusText: 'Unauthorized' });

    expect(status).toBe(401);
    expect(service.isLoggedIn()).toBe(false);
    expect(localStorage.getItem('jwt_token')).toBeNull();
  });

  it('logout borra el token', () => {
    service.login('admin', 'Admin123!').subscribe();
    http
      .expectOne(`${API_URL}/auth/login`)
      .flush({ token: 't', expiresAt: '2030-01-01T00:00:00Z', username: 'admin' });

    service.logout();

    expect(service.isLoggedIn()).toBe(false);
    expect(localStorage.getItem('jwt_token')).toBeNull();
  });
});
