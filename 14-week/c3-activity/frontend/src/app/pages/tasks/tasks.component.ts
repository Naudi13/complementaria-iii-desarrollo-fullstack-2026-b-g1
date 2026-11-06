// pages/tasks/tasks.component.ts
// Vista protegida con datos: lista, crea, marca y borra tareas, con estados carga/datos/error.
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Task, TasksService } from '../../core/tasks.service';

@Component({
  selector: 'app-tasks',
  imports: [FormsModule],
  templateUrl: './tasks.component.html',
})
export class TasksComponent implements OnInit {
  private readonly tasksService = inject(TasksService);

  readonly tasks = signal<Task[]>([]);
  readonly loading = signal(false);
  readonly error = signal<string | null>(null);
  newTitle = '';

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.error.set(null);
    this.tasksService.getAll().subscribe({
      next: (tasks) => {
        this.tasks.set(tasks);
        this.loading.set(false);
      },
      error: (err: HttpErrorResponse) => this.fail(err),
    });
  }

  add(): void {
    const title = this.newTitle.trim();
    if (!title) return;
    this.tasksService.create(title).subscribe({
      next: () => {
        this.newTitle = '';
        this.load();
      },
      error: (err: HttpErrorResponse) => this.fail(err),
    });
  }

  toggle(task: Task): void {
    this.tasksService.toggle(task.id).subscribe({
      next: () => this.load(),
      error: (err: HttpErrorResponse) => this.fail(err),
    });
  }

  remove(task: Task): void {
    this.tasksService.remove(task.id).subscribe({
      next: () => this.load(),
      error: (err: HttpErrorResponse) => this.fail(err),
    });
  }

  private fail(err: HttpErrorResponse): void {
    this.loading.set(false);
    // El 401 lo maneja el interceptor (cierra sesión y redirige); aquí solo se informa el resto.
    this.error.set(
      err.status === 0 ? 'No se pudo conectar con el servidor' : `Ocurrió un error (HTTP ${err.status})`,
    );
  }
}
