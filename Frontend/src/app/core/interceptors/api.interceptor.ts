import { HttpErrorResponse, HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, catchError, finalize, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';
import { LoadingService, NotificationService } from '../services/ui.service';

/**
 * Interceptor único de la aplicación. Pasa por él TODA petición HTTP y hace tres cosas:
 *  1. Adjunta el JWT y las cabeceras comunes.
 *  2. Enciende y apaga el indicador de "procesando".
 *  3. Centraliza el manejo de errores: muestra el mensaje y cierra la sesión si el token ya no sirve.
 */
@Injectable()
export class ApiInterceptor implements HttpInterceptor {
  constructor(
    private auth: AuthService,
    private loading: LoadingService,
    private notifications: NotificationService
  ) {}

  intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    const headers: Record<string, string> = { Accept: 'application/json' };

    const token = this.auth.token;
    if (token) {
      headers['Authorization'] = `Bearer ${token}`;
    }

    const authorizedRequest = request.clone({ setHeaders: headers });

    this.loading.start();

    return next.handle(authorizedRequest).pipe(
      catchError((error: HttpErrorResponse) => {
        this.handleError(error, request.url);
        return throwError(() => error);
      }),
      finalize(() => this.loading.stop())
    );
  }

  private handleError(error: HttpErrorResponse, url: string): void {
    const isLogin = url.includes('/auth/login');

    // 401 fuera del login = el token expiró o no es válido
    if (error.status === 401 && !isLogin) {
      this.notifications.warning('Su sesión expiró. Inicie sesión nuevamente');
      this.auth.clearSession();
      return;
    }

    if (error.status === 0) {
      this.notifications.error('No se pudo conectar con el servidor');
      return;
    }

    if (error.status === 403 && !error.error?.message) {
      this.notifications.error('No tiene permisos para realizar esta acción');
      return;
    }

    this.notifications.error(error.error?.message ?? 'Ocurrió un error inesperado');
  }
}
