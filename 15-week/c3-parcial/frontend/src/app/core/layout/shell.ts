import { Component, inject } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from '../auth/auth.service';

/** Layout de las páginas autenticadas: barra superior con menú según el rol. */
@Component({
  selector: 'app-shell',
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  template: `
    <header class="topbar">
      <a class="brand" [routerLink]="auth.homeUrl()">CitasMed</a>

      <nav class="menu">
        @if (auth.hasRole('Paciente')) {
          <a routerLink="/citas/mias" routerLinkActive="active">Mis citas</a>
          <a routerLink="/citas/reservar" routerLinkActive="active">Reservar</a>
        }
        @if (auth.hasRole('Medico')) {
          <a routerLink="/citas/agenda" routerLinkActive="active">Mi agenda</a>
        }
        @if (auth.hasRole('Admin')) {
          <a routerLink="/citas" routerLinkActive="active" [routerLinkActiveOptions]="{ exact: true }">Citas</a>
        }
        <a routerLink="/medicos" routerLinkActive="active">Médicos</a>
      </nav>

      @if (auth.usuario(); as usuario) {
        <div class="user">
          <span>{{ usuario.nombre }} <small class="rol">{{ usuario.rol }}</small></span>
          <button type="button" class="btn btn-ghost" (click)="auth.logout()">Salir</button>
        </div>
      }
    </header>

    <main class="container">
      <router-outlet />
    </main>
  `,
  styles: `
    .topbar {
      display: flex; align-items: center; gap: 1.5rem; flex-wrap: wrap;
      padding: .75rem 1.5rem; background: var(--primary); color: #fff;
    }
    .brand { font-weight: 700; font-size: 1.2rem; color: #fff; text-decoration: none; }
    .menu { display: flex; gap: 1rem; flex: 1; flex-wrap: wrap; }
    .menu a { color: #dbeafe; text-decoration: none; padding: .25rem 0; border-bottom: 2px solid transparent; }
    .menu a.active, .menu a:hover { color: #fff; border-color: #fff; }
    .user { display: flex; align-items: center; gap: .75rem; }
    .rol { background: rgba(255,255,255,.2); padding: .1rem .5rem; border-radius: 999px; margin-left: .25rem; }
    .btn-ghost { color: #fff; border-color: rgba(255,255,255,.5); }
  `,
})
export class Shell {
  protected readonly auth = inject(AuthService);
}
