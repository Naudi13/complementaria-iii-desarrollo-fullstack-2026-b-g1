import { Component, OnInit, computed, inject, input, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { getErrorMessage } from '../../../../core/http/api-error';
import { ErrorAlert } from '../../../../shared/components/error-alert';
import { LoadingSpinner } from '../../../../shared/components/loading-spinner';
import { Especialidad, Medico } from '../../../medicos/models/medico.model';
import { MedicoService } from '../../../medicos/services/medico.service';
import { CitaService } from '../../services/cita.service';

/** Franjas de 30 minutos entre 07:00 y 17:30 (mismas reglas que el backend). */
const FRANJAS: string[] = Array.from({ length: 22 }, (_, i) => {
  const minutos = 7 * 60 + i * 30;
  return `${String(Math.floor(minutos / 60)).padStart(2, '0')}:${String(minutos % 60).padStart(2, '0')}`;
});

function hoyIso(): string {
  const d = new Date();
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;
}

@Component({
  selector: 'app-reservar-cita-page',
  imports: [ReactiveFormsModule, RouterLink, LoadingSpinner, ErrorAlert],
  templateUrl: './reservar-cita-page.html',
})
export class ReservarCitaPage implements OnInit {
  private readonly citaService = inject(CitaService);
  private readonly medicoService = inject(MedicoService);
  private readonly router = inject(Router);

  /** ?medicoId= desde la lista de médicos (withComponentInputBinding). */
  readonly medicoId = input<string>();

  protected readonly franjas = FRANJAS;
  protected readonly hoy = hoyIso();

  protected readonly loading = signal(true);
  protected readonly saving = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly especialidades = signal<Especialidad[]>([]);
  protected readonly medicos = signal<Medico[]>([]);

  protected readonly form = inject(NonNullableFormBuilder).group({
    especialidadId: [0],
    medicoId: [0, Validators.min(1)],
    fecha: ['', Validators.required],
    hora: ['', Validators.required],
    motivo: ['', [Validators.required, Validators.maxLength(300)]],
  });

  private readonly especialidadSeleccionada = toSignal(this.form.controls.especialidadId.valueChanges, {
    initialValue: 0,
  });

  protected readonly medicosFiltrados = computed(() => {
    const esp = this.especialidadSeleccionada();
    return esp ? this.medicos().filter((m) => m.especialidadId === esp) : this.medicos();
  });

  ngOnInit(): void {
    this.medicoService.getEspecialidades().subscribe({
      next: (data) => this.especialidades.set(data),
    });

    this.medicoService
      .getAll()
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (data) => {
          this.medicos.set(data);
          const preseleccionado = Number(this.medicoId());
          if (data.some((m) => m.id === preseleccionado)) {
            this.form.controls.medicoId.setValue(preseleccionado);
          }
        },
        error: (err) => this.error.set(getErrorMessage(err)),
      });
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const { medicoId, fecha, hora, motivo } = this.form.getRawValue();

    this.saving.set(true);
    this.error.set(null);

    this.citaService
      .crear({ medicoId, fechaHora: `${fecha}T${hora}:00`, motivo })
      .pipe(finalize(() => this.saving.set(false)))
      .subscribe({
        next: () => void this.router.navigate(['/citas/mias']),
        // 400 (regla de negocio), 409 (horario ocupado)... se muestran tal cual los envía la API.
        error: (err) => this.error.set(getErrorMessage(err)),
      });
  }
}
