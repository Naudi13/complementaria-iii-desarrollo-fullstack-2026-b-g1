import { Component, OnInit, inject, signal } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { getErrorMessage } from '../../../../core/http/api-error';
import { ErrorAlert } from '../../../../shared/components/error-alert';
import { Especialidad } from '../../models/medico.model';
import { MedicoService } from '../../services/medico.service';

@Component({
  selector: 'app-medico-form-page',
  imports: [ReactiveFormsModule, RouterLink, ErrorAlert],
  templateUrl: './medico-form-page.html',
})
export class MedicoFormPage implements OnInit {
  private readonly medicoService = inject(MedicoService);
  private readonly router = inject(Router);

  protected readonly saving = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly especialidades = signal<Especialidad[]>([]);

  protected readonly form = inject(NonNullableFormBuilder).group({
    nombre: ['', [Validators.required, Validators.minLength(3)]],
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(8)]],
    registroProfesional: ['', Validators.required],
    especialidadId: [0, Validators.min(1)],
  });

  ngOnInit(): void {
    this.medicoService.getEspecialidades().subscribe({
      next: (data) => this.especialidades.set(data),
      error: (err) => this.error.set(getErrorMessage(err)),
    });
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.saving.set(true);
    this.error.set(null);

    this.medicoService
      .crear(this.form.getRawValue())
      .pipe(finalize(() => this.saving.set(false)))
      .subscribe({
        next: () => void this.router.navigate(['/medicos']),
        error: (err) => this.error.set(getErrorMessage(err)),
      });
  }
}
