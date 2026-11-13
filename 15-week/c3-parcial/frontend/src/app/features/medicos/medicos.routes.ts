import { Routes } from '@angular/router';
import { roleGuard } from '../../core/auth/role.guard';

export const MEDICOS_ROUTES: Routes = [
  {
    path: '',
    title: 'Médicos · CitasMed',
    loadComponent: () =>
      import('./pages/medico-list-page/medico-list-page').then((m) => m.MedicoListPage),
  },
  {
    path: 'nuevo',
    title: 'Nuevo médico · CitasMed',
    canActivate: [roleGuard('Admin')],
    loadComponent: () =>
      import('./pages/medico-form-page/medico-form-page').then((m) => m.MedicoFormPage),
  },
];
