import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { getErrorMessage } from '../../../../core/http/api-error';
import { ErrorAlert } from '../../../../shared/components/error-alert';
import { LoadingSpinner } from '../../../../shared/components/loading-spinner';
import { CitaList } from '../../components/cita-list/cita-list';
import { Cita } from '../../models/cita.model';
import { CitaService } from '../../services/cita.service';

/**
 * Ejemplo completo de consumo de un endpoint protegido:
 * 1. El servicio llama GET /api/citas/mias (el interceptor agrega el Bearer token).
 * 2. loading = true mientras llega la respuesta → spinner.
 * 3. Éxito → se guardan los datos. Error → mensaje + botón reintentar.
 * 4. finalize apaga el loading en ambos casos.
 */
@Component({
  selector: 'app-mis-citas-page',
  imports: [RouterLink, LoadingSpinner, ErrorAlert, CitaList],
  template: `
    <div class="page-header">
      <h1>Mis citas</h1>
      <a routerLink="/citas/reservar" class="btn btn-primary">Reservar cita</a>
    </div>

    @if (actionError(); as message) {
      <app-error-alert [message]="message" />
    }

    @if (loading()) {
      <app-loading-spinner message="Cargando tus citas..." />
    } @else if (error(); as message) {
      <app-error-alert [message]="message" [retryable]="true" (retry)="load()" />
    } @else {
      <app-cita-list
        [citas]="citas()"
        [puedeCancelar]="true"
        [procesandoId]="procesandoId()"
        mensajeVacio="Aún no tienes citas. ¡Reserva la primera!"
        (cancelar)="cancelar($event)"
      />
    }
  `,
})
export class MisCitasPage implements OnInit {
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
      .getMias()
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (data) => this.citas.set(data),
        error: (err) => this.error.set(getErrorMessage(err)),
      });
  }

  cancelar(cita: Cita): void {
    if (!confirm('¿Cancelar esta cita?')) return;

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
