import { Routes } from '@angular/router';
import { roleGuard } from '../../core/auth/role.guard';

export const CITAS_ROUTES: Routes = [
  {
    path: '',
    title: 'Citas · CitasMed',
    canActivate: [roleGuard('Admin')],
    loadComponent: () =>
      import('./pages/todas-citas-page/todas-citas-page').then((m) => m.TodasCitasPage),
  },
  {
    path: 'mias',
    title: 'Mis citas · CitasMed',
    canActivate: [roleGuard('Paciente')],
    loadComponent: () => import('./pages/mis-citas-page/mis-citas-page').then((m) => m.MisCitasPage),
  },
  {
    path: 'reservar',
    title: 'Reservar cita · CitasMed',
    canActivate: [roleGuard('Paciente')],
    loadComponent: () =>
      import('./pages/reservar-cita-page/reservar-cita-page').then((m) => m.ReservarCitaPage),
  },
  {
    path: 'agenda',
    title: 'Mi agenda · CitasMed',
    canActivate: [roleGuard('Medico')],
    loadComponent: () => import('./pages/agenda-page/agenda-page').then((m) => m.AgendaPage),
  },
];
