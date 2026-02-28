import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ToastService, Toast } from '../../../core/services/toast.service';

@Component({
  selector: 'app-toast-container',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="toast-container position-fixed d-flex flex-column gap-2 p-3" style="top: 80px; right: 0; z-index: 9999;">
      @for (toast of toastService.toasts(); track toast.id) {
        <div class="toast show align-items-center shadow-lg border-0 rounded-4 overflow-hidden"
             [class.toast-entering]="!toast.removing"
             [class.toast-leaving]="toast.removing"
             [ngStyle]="{
               'background-color': toast.type === 'success' ? '#f0fdf4' :
                                   toast.type === 'error' ? '#fef2f2' :
                                   toast.type === 'warning' ? '#fffbeb' : '#f0f9ff',
               'border-left': toast.type === 'success' ? '4px solid #198754' :
                              toast.type === 'error' ? '4px solid #dc3545' :
                              toast.type === 'warning' ? '4px solid #ffc107' : '4px solid #0dcaf0'
             }"
             role="alert" aria-live="assertive" aria-atomic="true"
             style="min-width: 300px; max-width: 380px;">
          <div class="d-flex w-100 p-3">
            <div class="toast-icon me-3 d-flex align-items-center justify-content-center text-white rounded-circle shadow-sm"
                 [ngClass]="{
                   'bg-success': toast.type === 'success',
                   'bg-danger': toast.type === 'error',
                   'bg-info': toast.type === 'info',
                   'bg-warning': toast.type === 'warning'
                 }"
                 style="width: 34px; height: 34px; min-width: 34px; font-size: 0.85rem;">
              <i class="fa-solid" [ngClass]="{
                'fa-check': toast.type === 'success',
                'fa-xmark': toast.type === 'error',
                'fa-info': toast.type === 'info',
                'fa-exclamation': toast.type === 'warning'
              }"></i>
            </div>
            <div class="toast-body p-0 d-flex flex-column justify-content-center flex-grow-1"
                 [ngClass]="{
                   'text-success': toast.type === 'success',
                   'text-danger': toast.type === 'error',
                   'text-info': toast.type === 'info',
                   'text-warning': toast.type === 'warning'
                 }">
              <strong *ngIf="toast.title" class="fw-bold mb-1" style="font-size: 0.9rem;">{{ toast.title }}</strong>
              <span class="fw-medium lh-sm" style="font-size: 0.88rem; color: #333;">{{ toast.message }}</span>
            </div>
            <button type="button" class="btn-close ms-3 my-auto shadow-none flex-shrink-0"
                    style="font-size: 0.7rem;"
                    (click)="removeToast(toast.id)" aria-label="Close"></button>
          </div>
          <!-- Progress bar -->
          <div class="toast-progress"
               [ngClass]="{
                 'bg-success': toast.type === 'success',
                 'bg-danger': toast.type === 'error',
                 'bg-info': toast.type === 'info',
                 'bg-warning': toast.type === 'warning'
               }"
               [style.animation-duration]="(toast.duration || 3000) + 'ms'">
          </div>
        </div>
      }
    </div>
  `,
  styles: [`
    /* ===== Enter Animation ===== */
    @keyframes toastSlideIn {
      from {
        transform: translateX(110%);
        opacity: 0;
      }
      to {
        transform: translateX(0);
        opacity: 1;
      }
    }

    /* ===== Exit Animation ===== */
    @keyframes toastSlideOut {
      from {
        transform: translateX(0);
        opacity: 1;
        max-height: 200px;
        margin-bottom: 0.5rem;
      }
      to {
        transform: translateX(110%);
        opacity: 0;
        max-height: 0;
        margin-bottom: 0;
      }
    }

    /* ===== Progress bar countdown ===== */
    @keyframes toastProgress {
      from { width: 100%; }
      to   { width: 0%; }
    }

    .toast-entering {
      animation: toastSlideIn 0.45s cubic-bezier(0.16, 1, 0.3, 1) forwards;
    }

    .toast-leaving {
      animation: toastSlideOut 0.4s cubic-bezier(0.55, 0, 1, 0.45) forwards;
      pointer-events: none;
    }

    .toast-progress {
      height: 3px;
      width: 100%;
      opacity: 0.5;
      animation: toastProgress linear forwards;
      transform-origin: left;
      border-radius: 0 0 8px 0;
    }
  `]
})
export class ToastContainerComponent {
  toastService = inject(ToastService);

  removeToast(id: number) {
    this.toastService.startRemove(id);
  }
}
