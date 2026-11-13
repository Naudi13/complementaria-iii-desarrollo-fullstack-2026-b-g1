import { Component, input } from '@angular/core';

@Component({
  selector: 'app-loading-spinner',
  template: `
    <div class="loading" role="status" aria-live="polite">
      <span class="spinner"></span>
      <span>{{ message() }}</span>
    </div>
  `,
  styles: `
    .loading { display: flex; align-items: center; gap: .75rem; padding: 1.5rem; color: var(--muted); }
    .spinner {
      width: 1.25rem; height: 1.25rem; border-radius: 50%;
      border: 3px solid var(--border); border-top-color: var(--primary);
      animation: spin .8s linear infinite;
    }
    @keyframes spin { to { transform: rotate(360deg); } }
  `,
})
export class LoadingSpinner {
  readonly message = input('Cargando...');
}
