// core/tasks.service.ts
// Acceso a la API de tareas (GET, POST, PATCH, DELETE). El token lo añade el interceptor.
import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';
import { API_URL } from '../config';

export interface Task {
  id: number;
  title: string;
  done: boolean;
}

@Injectable({ providedIn: 'root' })
export class TasksService {
  private readonly http = inject(HttpClient);
  private readonly url = `${API_URL}/tasks`;

  getAll(): Observable<Task[]> {
    return this.http.get<{ data: Task[] }>(this.url).pipe(map((r) => r.data));
  }

  create(title: string): Observable<Task> {
    return this.http.post<{ data: Task }>(this.url, { title }).pipe(map((r) => r.data));
  }

  toggle(id: number): Observable<Task> {
    return this.http.patch<{ data: Task }>(`${this.url}/${id}/toggle`, {}).pipe(map((r) => r.data));
  }

  remove(id: number): Observable<unknown> {
    return this.http.delete(`${this.url}/${id}`);
  }
}
