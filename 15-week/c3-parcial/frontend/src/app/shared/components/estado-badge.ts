import { Component, computed, input } from '@angular/core';

@Component({
  selector: 'app-estado-badge',
  template: `<span class="badge" [class]="cssClass()">{{ estado() }}</span>`,
  styles: `
    .badge { padding: .15rem .6rem; border-radius: 999px; font-size: .8rem; font-weight: 600; }
    .pendiente { background: #fef3c7; color: #92400e; }
    .atendida { background: #dcfce7; color: #166534; }
    .cancelada { background: #fee2e2; color: #991b1b; }
  `,
})
export class EstadoBadge {
  readonly estado = input.required<string>();
  protected readonly cssClass = computed(() => `badge ${this.estado().toLowerCase()}`);
}
