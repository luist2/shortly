import { Injectable } from '@angular/core';
import {
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpInterceptor,
  HttpErrorResponse,
} from '@angular/common/http';

import { Observable, throwError, BehaviorSubject } from 'rxjs';
import { catchError, filter, take, switchMap } from 'rxjs/operators';
import { AuthService } from '../services/auth.service';
import { Router } from '@angular/router';
import { STORAGE_KEYS } from '../constants/storage-keys.constants';
import { TokenResponse } from 'src/app/models/auth.model';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {
  private isRefreshing = false;
  private refreshTokenSubject: BehaviorSubject<string | null> = new BehaviorSubject<string | null>(
    null
  );

  constructor(private authService: AuthService, private router: Router) {}

  intercept(
    req: HttpRequest<unknown>,
    next: HttpHandler
  ): Observable<HttpEvent<unknown>> {
    // No agregar token a las peticiones de autenticación
    if (this.isAuthRequest(req)) {
      return next.handle(req);
    }

    // Agregar token a las peticiones autenticadas
    const token = this.authService.getToken();
    if (token) {
      req = this.addToken(req, token);
    }

    return next.handle(req).pipe(
      catchError((error: HttpErrorResponse) => {
        if (error.status === 401 && !this.isAuthRequest(req)) {
          return this.handle401Error(req, next);
        }
        // Otros errores
        return throwError(() => error);
      })
    );
  }

  private addToken(request: HttpRequest<any>, token: string): HttpRequest<any> {
    return request.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`,
      },
    });
  }

  private isAuthRequest(request: HttpRequest<any>): boolean {
    // No agregar token a las solicitudes de autenticación
    return (
      request.url.includes('/Auth/login') ||
      request.url.includes('/Auth/register') ||
      request.url.includes('/Auth/refresh-tokens')
    );
  }

  private handle401Error(
    request: HttpRequest<any>,
    next: HttpHandler
  ): Observable<HttpEvent<any>> {
    if (!this.isRefreshing) {
      this.isRefreshing = true;
      this.refreshTokenSubject.next(null);

      const refreshToken = localStorage.getItem(STORAGE_KEYS.REFRESH_TOKEN);
      const userId = this.authService.getUserId();

      if (refreshToken && userId) {
        return this.authService.refreshToken().pipe(
          switchMap((tokenResponse: TokenResponse) => {
            this.isRefreshing = false;
            this.refreshTokenSubject.next(tokenResponse.accessToken);
            return next.handle(
              this.addToken(request, tokenResponse.accessToken)
            );
          }),
          catchError((err) => {
            this.isRefreshing = false;
            this.authService.logout();
            this.router.navigate(['/login']);
            return throwError(() => err);
          })
        );
      } else {
        this.isRefreshing = false;
        this.authService.logout();
        this.router.navigate(['/login']);
        return throwError(() => new Error('No refresh token or userId found'));
      }
    } else {
      // Si ya se está refrescando el token, esperar hasta que se complete
      return this.refreshTokenSubject.pipe(
  filter((token): token is string => token != null),
  take(1),
  switchMap((token) => {
    return next.handle(this.addToken(request, token));
  })
);
    }
  }
}
