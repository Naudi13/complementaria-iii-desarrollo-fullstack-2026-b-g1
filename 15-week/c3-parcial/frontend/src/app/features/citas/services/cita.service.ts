import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { Cita, CrearCitaRequest } from '../models/cita.model';

@Injectable({ providedIn: 'root' })
export class CitaService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/citas`;

  /** Paciente: sus citas. */
  getMias(): Observable<Cita[]> {
    return this.http.get<Cita[]>(`${this.baseUrl}/mias`);
  }

  /** Médico: su agenda. */
  getAgenda(): Observable<Cita[]> {
    return this.http.get<Cita[]>(`${this.baseUrl}/agenda`);
  }

  /** Admin: todas las citas. */
  getTodas(): Observable<Cita[]> {
    return this.http.get<Cita[]>(this.baseUrl);
  }

  crear(request: CrearCitaRequest): Observable<Cita> {
    return this.http.post<Cita>(this.baseUrl, request);
  }

  cancelar(id: number): Observable<Cita> {
    return this.http.patch<Cita>(`${this.baseUrl}/${id}/cancelar`, {});
  }

  atender(id: number): Observable<Cita> {
    return this.http.patch<Cita>(`${this.baseUrl}/${id}/atender`, {});
  }
}
