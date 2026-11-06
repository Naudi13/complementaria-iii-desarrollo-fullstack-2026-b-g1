// Pruebas de la vista de tareas: estado "datos" y estado "error".
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { API_URL } from '../../config';
import { TasksComponent } from './tasks.component';

describe('TasksComponent', () => {
  let fixture: ComponentFixture<TasksComponent>;
  let http: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TasksComponent],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();

    fixture = TestBed.createComponent(TasksComponent);
    http = TestBed.inject(HttpTestingController);
    fixture.detectChanges(); // dispara ngOnInit -> GET /tasks
  });

  afterEach(() => http.verify());

  it('muestra "cargando" y luego la lista de tareas devuelta por la API', async () => {
    expect(fixture.nativeElement.querySelector('[data-testid="loading"]')).not.toBeNull();

    http.expectOne(`${API_URL}/tasks`).flush({
      ok: true,
      data: [
        { id: 1, title: 'Tarea uno', done: false },
        { id: 2, title: 'Tarea dos', done: true },
      ],
    });
    await fixture.whenStable();
    fixture.detectChanges();

    const items = fixture.nativeElement.querySelectorAll('[data-testid="task-item"]');
    expect(items.length).toBe(2);
    expect(items[0].textContent).toContain('Tarea uno');
    expect(fixture.nativeElement.querySelector('[data-testid="loading"]')).toBeNull();
  });

  // CASO DE ERROR: la API falla -> se muestra el mensaje y no hay tareas.
  it('muestra un mensaje de error cuando la API responde 500', async () => {
    http
      .expectOne(`${API_URL}/tasks`)
      .flush({ ok: false }, { status: 500, statusText: 'Server Error' });
    await fixture.whenStable();
    fixture.detectChanges();

    const error = fixture.nativeElement.querySelector('[data-testid="tasks-error"]');
    expect(error).not.toBeNull();
    expect(error.textContent).toContain('500');
    expect(fixture.nativeElement.querySelectorAll('[data-testid="task-item"]').length).toBe(0);
  });
});
