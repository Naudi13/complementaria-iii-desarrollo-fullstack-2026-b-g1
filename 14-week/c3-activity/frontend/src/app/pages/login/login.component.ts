// pages/login/login.component.ts
// Vista de login: formulario usuario/contraseña. Si la API responde 401 muestra el error.
import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../core/auth.service';

@Component({
  selector: 'app-login',
  imports: [FormsModule],
  templateUrl: './login.component.html',
})
export class LoginComponent {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  username = '';
  password = '';
  readonly loading = signal(false);
  readonly error = signal<string | null>(null);

  submit(): void {
    if (!this.username.trim() || !this.password) {
      this.error.set('Ingresa usuario y contraseña');
      return;
    }

    this.loading.set(true);
    this.error.set(null);

    this.auth.login(this.username.trim(), this.password).subscribe({
      next: () => {
        this.loading.set(false);
        this.router.navigate(['/tasks']);
      },
      error: (err: HttpErrorResponse) => {
        this.loading.set(false);
        this.error.set(
          err.status === 401
            ? 'Usuario o contraseña incorrectos'
            : 'No se pudo conectar con el servidor',
        );
      },
    });
  }
}
