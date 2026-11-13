import { Component, OnInit, inject, signal } from '@angular/core';
import { finalize } from 'rxjs';
import { getErrorMessage } from '../../../../core/http/api-error';
import { ErrorAlert } from '../../../../shared/components/error-alert';
import { LoadingSpinner } from '../../../../shared/components/loading-spinner';
import { CitaList } from '../../components/cita-list/cita-list';
import { Cita } from '../../models/cita.model';
import { CitaService } from '../../services/cita.service';

@Component({
  selector: 'app-todas-citas-page',
  imports: [LoadingSpinner, ErrorAlert, CitaList],
  template: `
    <div class="page-header"><h1>Todas las citas</h1></div>

    @if (actionError(); as message) {
      <app-error-alert [message]="message" />
    }

    @if (loading()) {
      <app-loading-spinner message="Cargando citas..." />
    } @else if (error(); as message) {
      <app-error-alert [message]="message" [retryable]="true" (retry)="load()" />
    } @else {
      <app-cita-list
        [citas]="citas()"
        [mostrarPaciente]="true"
        [puedeCancelar]="true"
        [procesandoId]="procesandoId()"
        (cancelar)="cancelar($event)"
      />
    }
  `,
})
export class TodasCitasPage implements OnInit {
  private readonly citaService = inject(CitaService);

  protected readonly loading = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly citas = signal<Cita[]>([]);

  protected readonly procesandoId = signal<number | null>(null);
  protected readonly actionError = signal<string | null>(null);

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.error.set(null);

    this.citaService
      .getTodas()
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (data) => this.citas.set(data),
        error: (err) => this.error.set(getErrorMessage(err)),
      });
  }

  cancelar(cita: Cita): void {
    if (!confirm(`¿Cancelar la cita de ${cita.paciente}?`)) return;

    this.procesandoId.set(cita.id);
    this.actionError.set(null);

    this.citaService
      .cancelar(cita.id)
      .pipe(finalize(() => this.procesandoId.set(null)))
      .subscribe({
        next: (actualizada) =>
          this.citas.update((list) => list.map((c) => (c.id === actualizada.id ? actualizada : c))),
        error: (err) => this.actionError.set(getErrorMessage(err)),
      });
  }
}
