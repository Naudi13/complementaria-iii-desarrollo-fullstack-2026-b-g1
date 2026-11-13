import { inject } from '@angular/core';
import { Routes } from '@angular/router';
import { authGuard } from './core/auth/auth.guard';
import { AuthService } from './core/auth/auth.service';
import { Shell } from './core/layout/shell';

/**
 * Cada feature se carga de forma diferida (lazy loading) con su propio archivo de rutas.
 * Todo lo que está bajo el Shell exige sesión (authGuard); cada ruta añade su roleGuard.
 */
export const routes: Routes = [
  {
    path: 'login',
    loadChildren: () => import('./features/auth/auth.routes').then((m) => m.AUTH_ROUTES),
  },
  {
    path: '',
    component: Shell,
    canActivate: [authGuard],
    children: [
      {
        path: 'citas',
        loadChildren: () => import('./features/citas/citas.routes').then((m) => m.CITAS_ROUTES),
      },
      {
        path: 'medicos',
        loadChildren: () => import('./features/medicos/medicos.routes').then((m) => m.MEDICOS_ROUTES),
      },
      // Inicio: depende del rol (Paciente → mis citas, Medico → agenda, Admin → citas).
      { path: '', pathMatch: 'full', redirectTo: () => inject(AuthService).homeUrl() },
    ],
  },
  { path: '**', redirectTo: '' },
];
