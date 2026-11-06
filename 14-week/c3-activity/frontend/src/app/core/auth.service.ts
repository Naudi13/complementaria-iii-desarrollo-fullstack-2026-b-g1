// core/auth.service.ts
// Se encarga de todo lo relacionado con la sesión: login, logout y guardar el JWT.
import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { API_URL } from '../config';

export interface LoginResponse {
  token: string;
  expiresAt: string;
  username: string;
}

const TOKEN_KEY = 'jwt_token';
const USER_KEY = 'jwt_user';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);

  private readonly _token = signal<string | null>(this.read(TOKEN_KEY));
  private readonly _username = signal<string | null>(this.read(USER_KEY));

  readonly token = this._token.asReadonly();
  readonly username = this._username.asReadonly();
  readonly isLoggedIn = computed(() => this._token() !== null);

  login(username: string, password: string): Observable<LoginResponse> {
    return this.http
      .post<LoginResponse>(`${API_URL}/auth/login`, { username, password })
      .pipe(tap((res) => this.saveSession(res)));
  }

  logout(): void {
    this._token.set(null);
    this._username.set(null);
    this.remove(TOKEN_KEY);
    this.remove(USER_KEY);
  }

  private saveSession(res: LoginResponse): void {
    this._token.set(res.token);
    this._username.set(res.username);
    this.write(TOKEN_KEY, res.token);
    this.write(USER_KEY, res.username);
  }

  // localStorage puede no existir (modo privado, SSR, tests): se envuelve en try/catch.
  private read(key: string): string | null {
    try {
      return localStorage.getItem(key);
    } catch {
      return null;
    }
  }
  private write(key: string, value: string): void {
    try {
      localStorage.setItem(key, value);
    } catch {
      /* sin almacenamiento: la sesión vive solo en memoria */
    }
  }
  private remove(key: string): void {
    try {
      localStorage.removeItem(key);
    } catch {
      /* nada que borrar */
    }
  }
}
