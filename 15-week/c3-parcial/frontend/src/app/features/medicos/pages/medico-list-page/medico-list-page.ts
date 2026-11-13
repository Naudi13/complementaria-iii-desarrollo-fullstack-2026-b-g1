import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { AuthService } from '../../../../core/auth/auth.service';
import { getErrorMessage } from '../../../../core/http/api-error';
import { ErrorAlert } from '../../../../shared/components/error-alert';
import { LoadingSpinner } from '../../../../shared/components/loading-spinner';
import { Especialidad, Medico } from '../../models/medico.model';
import { MedicoService } from '../../services/medico.service';

@Component({
  selector: 'app-medico-list-page',
  imports: [FormsModule, RouterLink, LoadingSpinner, ErrorAlert],
  templateUrl: './medico-list-page.html',
})
export class MedicoListPage implements OnInit {
  private readonly medicoService = inject(MedicoService);
  protected readonly auth = inject(AuthService);

  // Estado de la pantalla: carga · error · datos
  protected readonly loading = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly medicos = signal<Medico[]>([]);

  protected readonly especialidades = signal<Especialidad[]>([]);
  protected readonly especialidadId = signal<number | null>(null);
  protected readonly eliminandoId = signal<number | null>(null);
  protected readonly actionError = signal<string | null>(null);

  ngOnInit(): void {
    this.medicoService.getEspecialidades().subscribe({
      next: (data) => this.especialidades.set(data),
      error: () => this.especialidades.set([]),
    });
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.error.set(null);

    this.medicoService
      .getAll(this.especialidadId())
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (data) => this.medicos.set(data),
        error: (err) => this.error.set(getErrorMessage(err)),
      });
  }

  filtrar(especialidadId: number | null): void {
    this.especialidadId.set(especialidadId);
    this.load();
  }

  eliminar(medico: Medico): void {
    if (!confirm(`¿Eliminar a ${medico.nombre}?`)) return;

    this.eliminandoId.set(medico.id);
    this.actionError.set(null);

    this.medicoService
      .eliminar(medico.id)
      .pipe(finalize(() => this.eliminandoId.set(null)))
      .subscribe({
        next: () => this.medicos.update((list) => list.filter((m) => m.id !== medico.id)),
        error: (err) => this.actionError.set(getErrorMessage(err)),
      });
  }
}
