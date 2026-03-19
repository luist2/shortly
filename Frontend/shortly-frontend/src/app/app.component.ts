import { Component } from '@angular/core';
import { AuthService } from './core/services/auth.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss'],
})
export class AppComponent {
  title = 'shortly-frontend';

  constructor(private router: Router, private authService: AuthService) {}

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
