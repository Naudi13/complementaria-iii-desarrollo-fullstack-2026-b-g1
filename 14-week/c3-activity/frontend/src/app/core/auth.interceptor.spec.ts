// Pruebas del interceptor: añade el token y reacciona al 401.
import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { API_URL } from '../config';
import { AuthService } from './auth.service';
import { authInterceptor } from './auth.interceptor';

describe('authInterceptor', () => {
  let http: HttpClient;
  let controller: HttpTestingController;
  let auth: AuthService;
  let router: Router;

  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({
      providers: [
        provideRouter([]),
        provideHttpClient(withInterceptors([authInterceptor])),
        provideHttpClientTesting(),
      ],
    });
    http = TestBed.inject(HttpClient);
    controller = TestBed.inject(HttpTestingController);
    auth = TestBed.inject(AuthService);
    router = TestBed.inject(Router);
  });

  afterEach(() => controller.verify());

  function startSession() {
    auth.login('admin', 'Admin123!').subscribe();
    controller
      .expectOne(`${API_URL}/auth/login`)
      .flush({ token: 'mi.token.jwt', expiresAt: '2030-01-01T00:00:00Z', username: 'admin' });
  }

  it('añade el header Authorization a las peticiones hacia la API', () => {
    startSession();

    http.get(`${API_URL}/tasks`).subscribe();

    const req = controller.expectOne(`${API_URL}/tasks`);
    expect(req.request.headers.get('Authorization')).toBe('Bearer mi.token.jwt');
    req.flush({});
  });

  it('NO envía el token a otros dominios', () => {
    startSession();

    http.get('https://otro-sitio.com/datos').subscribe();

    const req = controller.expectOne('https://otro-sitio.com/datos');
    expect(req.request.headers.has('Authorization')).toBe(false);
    req.flush({});
  });

  // CASO DE ERROR: token vencido/inválido -> el servidor responde 401.
  it('ante un 401 cierra la sesión y redirige a /login', () => {
    startSession();
    const navigate = vi.spyOn(router, 'navigate').mockResolvedValue(true);

    http.get(`${API_URL}/tasks`).subscribe({ error: () => undefined });
    controller
      .expectOne(`${API_URL}/tasks`)
      .flush({}, { status: 401, statusText: 'Unauthorized' });

    expect(auth.isLoggedIn()).toBe(false);
    expect(navigate).toHaveBeenCalledWith(['/login']);
  });
});
