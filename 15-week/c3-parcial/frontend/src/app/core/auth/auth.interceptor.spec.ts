import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { environment } from '../../../environments/environment';
import { authInterceptor } from './auth.interceptor';
import { AuthService } from './auth.service';

/**
 * Prueba unitaria del interceptor: verifica que el token viaje en el header
 * Authorization SOLO hacia nuestra API.
 */
describe('authInterceptor', () => {
  let http: HttpClient;
  let httpMock: HttpTestingController;
  const authStub = { token: null as string | null };

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([authInterceptor])),
        provideHttpClientTesting(),
        { provide: AuthService, useValue: authStub },
      ],
    });
    http = TestBed.inject(HttpClient);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('agrega "Authorization: Bearer <token>" a las peticiones a la API', () => {
    authStub.token = 'token-de-prueba';

    http.get(`${environment.apiUrl}/citas/mias`).subscribe();

    const req = httpMock.expectOne(`${environment.apiUrl}/citas/mias`);
    expect(req.request.headers.get('Authorization')).toBe('Bearer token-de-prueba');
    req.flush([]);
  });

  it('no agrega el header si no hay sesión', () => {
    authStub.token = null;

    http.get(`${environment.apiUrl}/medicos`).subscribe();

    const req = httpMock.expectOne(`${environment.apiUrl}/medicos`);
    expect(req.request.headers.has('Authorization')).toBe(false);
    req.flush([]);
  });

  it('no envía el token a dominios externos', () => {
    authStub.token = 'token-de-prueba';

    http.get('https://api.externa.test/datos').subscribe();

    const req = httpMock.expectOne('https://api.externa.test/datos');
    expect(req.request.headers.has('Authorization')).toBe(false);
    req.flush({});
  });
});
