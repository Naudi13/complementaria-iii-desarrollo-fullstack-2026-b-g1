import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthResponse, LoginRequest, Rol } from './auth.models';

const STORAGE_KEY = 'citasmed.session';

/** Página de inicio de cada rol. */
const HOME_BY_ROL: Record<Rol, string> = {
  Paciente: '/citas/mias',
  Medico: '/citas/agenda',
  Admin: '/citas',
};

/**
 * Maneja la sesión del usuario: login, logout y acceso al token.
 *
 * Dónde se guarda el token: en localStorage, para que la sesión sobreviva a recargas.
 * Trade-off: localStorage es accesible desde JavaScript, así que un ataque XSS podría leerlo.
 * Mitigaciones: Angular escapa el HTML por defecto, el token expira (60 min) y no se
 * guardan datos sensibles en él. La alternativa más segura es una cookie HttpOnly + SameSite.
 */
@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);

  private readonly session = signal<AuthResponse | null>(this.readSession());

  readonly usuario = computed(() => this.session()?.usuario ?? null);
  readonly isAuthenticated = computed(() => this.session() !== null);

  login(request: LoginRequest): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(`${environment.apiUrl}/auth/login`, request)
      .pipe(tap((response) => this.saveSession(response)));
  }

  /** Cierra la sesión. Si expiró, lo indica en la pantalla de login. */
  logout(expired = false): void {
    this.clearSession();
    void this.router.navigate(['/login'], expired ? { queryParams: { expired: 1 } } : {});
  }

  /** Token vigente o null si no hay sesión o ya expiró. */
  get token(): string | null {
    const current = this.session();
    if (!current) return null;
    if (this.isExpired(current)) {
      this.clearSession();
      return null;
    }
    return current.accessToken;
  }

  hasValidSession(): boolean {
    return this.token !== null;
  }

  /** Autorización en la UI (la seguridad real la aplica el backend). */
  hasRole(...roles: Rol[]): boolean {
    const rol = this.usuario()?.rol;
    return rol !== undefined && roles.includes(rol);
  }

  homeUrl(): string {
    const rol = this.usuario()?.rol;
    return rol ? HOME_BY_ROL[rol] : '/login';
  }

  // ---------- almacenamiento ----------

  private saveSession(response: AuthResponse): void {
    this.session.set(response);
    try {
      localStorage.setItem(STORAGE_KEY, JSON.stringify(response));
    } catch {
      /* almacenamiento no disponible: la sesión vive solo en memoria */
    }
  }

  private clearSession(): void {
    this.session.set(null);
    try {
      localStorage.removeItem(STORAGE_KEY);
    } catch {
      /* ignorar */
    }
  }

  private readSession(): AuthResponse | null {
    try {
      const raw = localStorage.getItem(STORAGE_KEY);
      if (!raw) return null;
      const parsed = JSON.parse(raw) as AuthResponse;
      return this.isExpired(parsed) ? null : parsed;
    } catch {
      return null;
    }
  }

  private isExpired(session: AuthResponse): boolean {
    return new Date(session.expiresAt).getTime() <= Date.now();
  }
}
