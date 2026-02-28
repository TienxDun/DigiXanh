import { Injectable, signal } from '@angular/core';

export interface Toast {
    id: number;
    message: string;
    type: 'success' | 'error' | 'info' | 'warning';
    title?: string;
    duration?: number;
    removing?: boolean; // Trạng thái để kích hoạt animation biến mất
}

@Injectable({
    providedIn: 'root'
})
export class ToastService {
    private tempId = 0;

    // Using Angular Signal for reactive toasts
    toasts = signal<Toast[]>([]);

    show(toast: Omit<Toast, 'id'>) {
        const id = ++this.tempId;
        const newToast: Toast = { ...toast, id, removing: false };

        this.toasts.update(currentToasts => [...currentToasts, newToast]);

        // Auto dismiss
        const duration = toast.duration || 3000;
        setTimeout(() => {
            this.startRemove(id);
        }, duration);
    }

    success(message: string, title?: string, duration = 3000) {
        this.show({ message, title, type: 'success', duration });
    }

    error(message: string, title?: string, duration = 4000) {
        this.show({ message, title, type: 'error', duration });
    }

    info(message: string, title?: string, duration = 3000) {
        this.show({ message, title, type: 'info', duration });
    }

    /**
     * Đánh dấu toast đang biến mất (trigger exit animation),
     * sau 400ms mới thực sự xóa khỏi danh sách.
     */
    startRemove(id: number) {
        // Bước 1: Đánh dấu removing = true để trigger CSS exit animation
        this.toasts.update(currentToasts =>
            currentToasts.map(t => t.id === id ? { ...t, removing: true } : t)
        );

        // Bước 2: Sau khi animation chạy xong (400ms), mới thực sự xóa
        setTimeout(() => {
            this.remove(id);
        }, 400);
    }

    remove(id: number) {
        this.toasts.update(currentToasts => currentToasts.filter(t => t.id !== id));
    }
}
