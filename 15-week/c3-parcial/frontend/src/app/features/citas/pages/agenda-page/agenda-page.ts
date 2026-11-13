import { Component, OnInit, inject, signal } from '@angular/core';
import { finalize } from 'rxjs';
import { getErrorMessage } from '../../../../core/http/api-error';
import { ErrorAlert } from '../../../../shared/components/error-alert';
import { LoadingSpinner } from '../../../../shared/components/loading-spinner';
import { CitaList } from '../../components/cita-list/cita-list';
import { Cita } from '../../models/cita.model';
import { CitaService } from '../../services/cita.service';

@Component({
  selector: 'app-agenda-page',
  imports: [LoadingSpinner, ErrorAlert, CitaList],
  template: `
    <div class="page-header"><h1>Mi agenda</h1></div>

    @if (actionError(); as message) {
      <app-error-alert [message]="message" />
    }

    @if (loading()) {
      <app-loading-spinner message="Cargando agenda..." />
    } @else if (error(); as message) {
      <app-error-alert [message]="message" [retryable]="true" (retry)="load()" />
    } @else {
      <app-cita-list
        [citas]="citas()"
        [mostrarPaciente]="true"
        [mostrarMedico]="false"
        [puedeAtender]="true"
        [procesandoId]="procesandoId()"
        mensajeVacio="No tienes citas agendadas."
        (atender)="atender($event)"
      />
    }
  `,
})
export class AgendaPage implements OnInit {
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
      .getAgenda()
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (data) => this.citas.set(data),
        error: (err) => this.error.set(getErrorMessage(err)),
      });
  }

  atender(cita: Cita): void {
    this.procesandoId.set(cita.id);
    this.actionError.set(null);

    this.citaService
      .atender(cita.id)
      .pipe(finalize(() => this.procesandoId.set(null)))
      .subscribe({
        next: (actualizada) =>
          this.citas.update((list) => list.map((c) => (c.id === actualizada.id ? actualizada : c))),
        error: (err) => this.actionError.set(getErrorMessage(err)),
      });
  }
}
