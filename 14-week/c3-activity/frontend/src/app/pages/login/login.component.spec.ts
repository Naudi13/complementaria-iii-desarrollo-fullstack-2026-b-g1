// Pruebas de la vista de login: éxito (navega) y error (credenciales malas).
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { API_URL } from '../../config';
import { LoginComponent } from './login.component';

describe('LoginComponent', () => {
  let fixture: ComponentFixture<LoginComponent>;
  let component: LoginComponent;
  let http: HttpTestingController;
  let router: Router;

  beforeEach(async () => {
    localStorage.clear();
    await TestBed.configureTestingModule({
      imports: [LoginComponent],
      providers: [provideRouter([]), provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();

    fixture = TestBed.createComponent(LoginComponent);
    component = fixture.componentInstance;
    http = TestBed.inject(HttpTestingController);
    router = TestBed.inject(Router);
    fixture.detectChanges();
  });

  afterEach(() => http.verify());

  it('navega a /tasks cuando el login es correcto', () => {
    const navigate = vi.spyOn(router, 'navigate').mockResolvedValue(true);
    component.username = 'admin';
    component.password = 'Admin123!';

    component.submit();
    http
      .expectOne(`${API_URL}/auth/login`)
      .flush({ token: 't', expiresAt: '2030-01-01T00:00:00Z', username: 'admin' });

    expect(navigate).toHaveBeenCalledWith(['/tasks']);
  });

  // CASO DE ERROR: contraseña incorrecta -> 401 -> mensaje visible y sin navegación.
  it('muestra un error y no navega cuando las credenciales son incorrectas', async () => {
    const navigate = vi.spyOn(router, 'navigate').mockResolvedValue(true);
    component.username = 'admin';
    component.password = 'mala';

    component.submit();
    http
      .expectOne(`${API_URL}/auth/login`)
      .flush({ ok: false }, { status: 401, statusText: 'Unauthorized' });
    await fixture.whenStable();
    fixture.detectChanges();

    const error = fixture.nativeElement.querySelector('[data-testid="login-error"]');
    expect(error.textContent).toContain('incorrectos');
    expect(navigate).not.toHaveBeenCalled();
  });

  it('no llama a la API si faltan campos', () => {
    component.username = '';
    component.password = '';

    component.submit();

    http.expectNone(`${API_URL}/auth/login`);
    expect(component.error()).toContain('Ingresa');
  });
});
