import { Injectable } from '@angular/core';
import {
  ActivatedRouteSnapshot,
  CanActivate,
  Router,
  RouterStateSnapshot,
  UrlTree,
} from '@angular/router';

import { Observable } from 'rxjs';
import { AuthService } from '../services/auth.service';

/**
 * Guard que protege rutas que requieren autenticación.
 * Verifica si el usuario tiene un token válido antes de permitir el acceso.
 * Si no está autenticado, redirige al login.
 */
@Injectable({
  providedIn: 'root',
})
export class AuthGuard implements CanActivate {
  constructor(private authService: AuthService, private router: Router) {}

  canActivate(
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot
  ):
    | Observable<boolean | UrlTree>
    | Promise<boolean | UrlTree>
    | boolean
    | UrlTree {
    // Permitir acceso si está autenticado
    if (this.authService.isAuthenticated()) {
      return true;
    }

    // Redirigir al login si no está autenticado
    console.warn('User not authenticated. Redirecting to login.');
    return this.router.createUrlTree(['/login'], {
      queryParams: { returnUrl: state.url },
    });
  }
}
