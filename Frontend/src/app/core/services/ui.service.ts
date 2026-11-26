import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

export interface Toast {
  id: number;
  type: 'success' | 'danger' | 'warning';
  message: string;
}

@Injectable({ providedIn: 'root' })
export class NotificationService {
  private nextId = 1;
  private readonly toastsSubject = new BehaviorSubject<Toast[]>([]);
  readonly toasts$ = this.toastsSubject.asObservable();

  success(message: string): void {
    this.show('success', message);
  }

  error(message: string): void {
    this.show('danger', message, 7000);
  }

  warning(message: string): void {
    this.show('warning', message, 6000);
  }

  dismiss(id: number): void {
    this.toastsSubject.next(this.toastsSubject.value.filter((toast) => toast.id !== id));
  }

  private show(type: Toast['type'], message: string, duration = 4000): void {
    const toast: Toast = { id: this.nextId++, type, message };
    this.toastsSubject.next([...this.toastsSubject.value, toast]);
    setTimeout(() => this.dismiss(toast.id), duration);
  }
}

@Injectable({ providedIn: 'root' })
export class LoadingService {
  private pending = 0;
  private readonly loadingSubject = new BehaviorSubject<boolean>(false);
  readonly loading$ = this.loadingSubject.asObservable();

  start(): void {
    this.pending++;
    setTimeout(() => this.loadingSubject.next(this.pending > 0));
  }

  stop(): void {
    this.pending = Math.max(0, this.pending - 1);
    setTimeout(() => this.loadingSubject.next(this.pending > 0));
  }
}
