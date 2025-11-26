import { Component } from '@angular/core';
import { AuthService } from './core/services/auth.service';
import { NavigationEnd, Router } from '@angular/router';
import { filter } from 'rxjs';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss'],
})
export class AppComponent {
  title = 'shortly-frontend';
  hideLayout = false; // Ocultar navbar en login y register

  constructor(private router: Router, private authService: AuthService) {
    // Detectar cambio de rutas
    this.router.events
      .pipe(filter((event) => event instanceof NavigationEnd))
      .subscribe(() => {
        const publicRoutes = ['/login', '/register'];
        this.hideLayout = publicRoutes.includes(this.router.url);
      });
  }

  isAuthenticated(): boolean {
    return this.authService.isAuthenticated();
  }

  getUserEmail(): string | null {
    return this.authService.getUserEmail();
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
