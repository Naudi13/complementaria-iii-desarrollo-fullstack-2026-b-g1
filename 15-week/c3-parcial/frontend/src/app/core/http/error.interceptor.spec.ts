import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { vi } from 'vitest';
import { environment } from '../../../environments/environment';
import { AuthService } from '../auth/auth.service';
import { errorInterceptor } from './error.interceptor';

describe('errorInterceptor', () => {
  let http: HttpClient;
  let httpMock: HttpTestingController;
  const authStub = { logout: vi.fn() };

  beforeEach(() => {
    authStub.logout.mockClear();
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([errorInterceptor])),
        provideHttpClientTesting(),
        { provide: AuthService, useValue: authStub },
      ],
    });
    http = TestBed.inject(HttpClient);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('cierra la sesión cuando la API responde 401 (token vencido o inválido)', () => {
    http.get(`${environment.apiUrl}/citas/mias`).subscribe({ error: () => undefined });

    httpMock.expectOne(`${environment.apiUrl}/citas/mias`).flush(null, { status: 401, statusText: 'Unauthorized' });

    expect(authStub.logout).toHaveBeenCalledWith(true);
  });

  it('no cierra la sesión con 403: el usuario está autenticado pero sin permiso', () => {
    http.get(`${environment.apiUrl}/usuarios`).subscribe({ error: () => undefined });

    httpMock.expectOne(`${environment.apiUrl}/usuarios`).flush(null, { status: 403, statusText: 'Forbidden' });

    expect(authStub.logout).not.toHaveBeenCalled();
  });

  it('no cierra la sesión si el 401 viene del propio login (credenciales incorrectas)', () => {
    http.post(`${environment.apiUrl}/auth/login`, {}).subscribe({ error: () => undefined });

    httpMock.expectOne(`${environment.apiUrl}/auth/login`).flush(null, { status: 401, statusText: 'Unauthorized' });

    expect(authStub.logout).not.toHaveBeenCalled();
  });
});
