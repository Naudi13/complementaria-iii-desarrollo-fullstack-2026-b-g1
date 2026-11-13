import { Component, input, output } from '@angular/core';

@Component({
  selector: 'app-error-alert',
  template: `
    <div class="alert alert-error" role="alert">
      <span>{{ message() }}</span>
      @if (retryable()) {
        <button type="button" class="btn" (click)="retry.emit()">Reintentar</button>
      }
    </div>
  `,
  styles: `
    .alert { display: flex; justify-content: space-between; align-items: center; gap: 1rem; }
  `,
})
export class ErrorAlert {
  readonly message = input.required<string>();
  readonly retryable = input(false);
  readonly retry = output<void>();
}
