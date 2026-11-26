import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';
import { Session } from '../models';

const STORAGE_KEY = 'gc_session';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private session: Session | null = this.readSession();

  constructor(private http: HttpClient, private router: Router) {}

  get currentSession(): Session | null {
    return this.session;
  }

  get token(): string | null {
    return this.session?.token ?? null;
  }

  get rolId(): number {
    return this.session?.rolId ?? 0;
  }

  isAuthenticated(): boolean {
    return !!this.session && new Date(this.session.expiresAt).getTime() > Date.now();
  }

  hasRole(...roles: number[]): boolean {
    return roles.includes(this.rolId);
  }

  login(userName: string, password: string): Observable<Session> {
    return this.http.post<Session>('/api/auth/login', { userName, password }).pipe(
      tap((session) => {
        this.session = session;
        sessionStorage.setItem(STORAGE_KEY, JSON.stringify(session));
      })
    );
  }

  recoverPassword(data: { userName: string; identification: string; email: string; newPassword: string }): Observable<{ message: string }> {
    return this.http.post<{ message: string }>('/api/auth/recover-password', data);
  }

  logout(): void {
    if (this.isAuthenticated()) {
      this.http.post('/api/auth/logout', {}).subscribe({ error: () => undefined });
    }
    this.clearSession();
  }

  clearSession(): void {
    this.session = null;
    sessionStorage.removeItem(STORAGE_KEY);
    this.router.navigate(['/auth/login']);
  }

  private readSession(): Session | null {
    const raw = sessionStorage.getItem(STORAGE_KEY);
    if (!raw) {
      return null;
    }

    try {
      return JSON.parse(raw) as Session;
    } catch {
      return null;
    }
  }
}
