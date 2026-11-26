import { Component, EventEmitter, Input, Output } from '@angular/core';

@Component({
  selector: 'app-modal',
  standalone: false,
  template: `
    <div class="gc-backdrop" (click)="closed.emit()"></div>
    <div class="gc-modal" [class.gc-modal-lg]="large" role="dialog" aria-modal="true">
      <div class="card">
        <div class="card-header d-flex justify-content-between align-items-center bg-white">
          <strong>{{ title }}</strong>
          <button type="button" class="btn-close" aria-label="Cerrar" (click)="closed.emit()"></button>
        </div>
        <div class="card-body">
          <ng-content></ng-content>
        </div>
      </div>
    </div>
  `,
  styles: [
    `
      .gc-backdrop {
        position: fixed;
        inset: 0;
        background: rgba(0, 0, 0, 0.45);
        z-index: 1040;
      }
      .gc-modal {
        position: fixed;
        top: 6vh;
        left: 50%;
        transform: translateX(-50%);
        width: min(560px, 94vw);
        max-height: 88vh;
        overflow-y: auto;
        z-index: 1050;
      }
      .gc-modal-lg {
        width: min(860px, 94vw);
      }
    `,
  ],
})
export class ModalComponent {
  @Input() title = '';
  @Input() large = false;
  @Output() closed = new EventEmitter<void>();
}
