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
 * Guard para rutas públicas (login, register).
 * Si el usuario ya está autenticado, lo redirige al dashboard.
 * Esto evita que usuarios logueados accedan a páginas de login/registro.
 */
@Injectable({
  providedIn: 'root',
})
export class PublicGuard implements CanActivate {
  constructor(private authService: AuthService, private router: Router) {}

  canActivate(
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot
  ):
    | Observable<boolean | UrlTree>
    | Promise<boolean | UrlTree>
    | boolean
    | UrlTree {
    // Si el usuario está autenticado, redirigir al dashboard
    if (this.authService.isAuthenticated()) {
      console.warn('User already authenticated. Redirecting to dashboard.');
      return this.router.createUrlTree(['/dashboard']);
    }

    // Permitir acceso a rutas públicas si no está autenticado
    return true;
  }
}
