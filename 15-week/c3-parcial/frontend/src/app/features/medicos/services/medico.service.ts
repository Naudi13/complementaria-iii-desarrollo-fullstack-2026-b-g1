import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { CrearMedicoRequest, Especialidad, Medico } from '../models/medico.model';

/**
 * Acceso a la API de médicos y especialidades.
 * El token lo agrega el authInterceptor: el servicio no se preocupa por la seguridad.
 */
@Injectable({ providedIn: 'root' })
export class MedicoService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/medicos`;

  getAll(especialidadId?: number | null): Observable<Medico[]> {
    let params = new HttpParams();
    if (especialidadId) params = params.set('especialidadId', especialidadId);
    return this.http.get<Medico[]>(this.baseUrl, { params });
  }

  getEspecialidades(): Observable<Especialidad[]> {
    return this.http.get<Especialidad[]>(`${environment.apiUrl}/especialidades`);
  }

  crear(request: CrearMedicoRequest): Observable<Medico> {
    return this.http.post<Medico>(this.baseUrl, request);
  }

  eliminar(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
