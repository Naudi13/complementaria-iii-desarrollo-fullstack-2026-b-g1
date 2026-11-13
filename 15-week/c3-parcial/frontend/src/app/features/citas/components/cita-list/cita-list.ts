import { DatePipe } from '@angular/common';
import { Component, input, output } from '@angular/core';
import { EstadoBadge } from '../../../../shared/components/estado-badge';
import { Cita } from '../../models/cita.model';

/**
 * Componente de presentación (sin lógica de datos): recibe citas y emite acciones.
 * Se reutiliza en "Mis citas", "Agenda" y "Todas las citas".
 */
@Component({
  selector: 'app-cita-list',
  imports: [DatePipe, EstadoBadge],
  template: `
    <div class="table-wrap">
      <table class="table">
        <thead>
          <tr>
            <th>Fecha</th>
            <th>Hora</th>
            @if (mostrarPaciente()) { <th>Paciente</th> }
            @if (mostrarMedico()) { <th>Médico</th> }
            <th>Motivo</th>
            <th>Estado</th>
            <th></th>
          </tr>
        </thead>
        <tbody>
          @for (cita of citas(); track cita.id) {
            <tr>
              <td>{{ cita.fechaHora | date: 'EEE d MMM y' }}</td>
              <td>{{ cita.fechaHora | date: 'HH:mm' }}</td>
              @if (mostrarPaciente()) { <td>{{ cita.paciente }}</td> }
              @if (mostrarMedico()) {
                <td>{{ cita.medico }}<br /><small class="muted">{{ cita.especialidad }}</small></td>
              }
              <td>{{ cita.motivo }}</td>
              <td><app-estado-badge [estado]="cita.estado" /></td>
              <td class="actions">
                @if (cita.estado === 'Pendiente') {
                  @if (puedeAtender()) {
                    <button type="button" class="btn btn-primary" [disabled]="procesandoId() === cita.id" (click)="atender.emit(cita)">
                      Atender
                    </button>
                  }
                  @if (puedeCancelar()) {
                    <button type="button" class="btn btn-danger" [disabled]="procesandoId() === cita.id" (click)="cancelar.emit(cita)">
                      Cancelar
                    </button>
                  }
                }
              </td>
            </tr>
          } @empty {
            <tr><td colspan="7" class="empty">{{ mensajeVacio() }}</td></tr>
          }
        </tbody>
      </table>
    </div>
  `,
  styles: `
    .actions { display: flex; gap: .5rem; justify-content: flex-end; }
  `,
})
export class CitaList {
  readonly citas = input.required<Cita[]>();
  readonly mostrarPaciente = input(false);
  readonly mostrarMedico = input(true);
  readonly puedeCancelar = input(false);
  readonly puedeAtender = input(false);
  readonly procesandoId = input<number | null>(null);
  readonly mensajeVacio = input('No hay citas registradas.');

  readonly cancelar = output<Cita>();
  readonly atender = output<Cita>();
}
