import { Routes } from '@angular/router';
import { authGuard } from './core/auth.guard';
import { LoginComponent } from './pages/login/login.component';
import { TasksComponent } from './pages/tasks/tasks.component';

export const routes: Routes = [
  { path: 'login', component: LoginComponent },
  { path: 'tasks', component: TasksComponent, canActivate: [authGuard] },
  { path: '', pathMatch: 'full', redirectTo: 'tasks' },
  { path: '**', redirectTo: 'tasks' },
];
