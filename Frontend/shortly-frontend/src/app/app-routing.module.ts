import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AuthGuard } from './core/guards/auth.guard';
import { PublicGuard } from './core/guards/public.guard';
import { LoginComponent } from './features/auth/login/login.component';
import { RegisterComponent } from './features/auth/register/register.component';
import { DashboardComponent } from './features/dashboard/dashboard.component';
import { UrlCreateComponent } from './features/url-create/url-create.component';
import { UrlStatsComponent } from './features/url-stats/url-stats.component';

const routes: Routes = [
  // Ruta raíz - Redirige según autenticación
  {
    path: '',
    redirectTo: '/dashboard',
    pathMatch: 'full',
  },

  // RUTAS PÚBLICAS (con PublicGuard)
  {
    path: 'login',
    component: LoginComponent,
    canActivate: [PublicGuard],
  },

  {
    path: 'register',
    component: RegisterComponent,
    canActivate: [PublicGuard],
  },

  // RUTAS PROTEGIDAS (con AuthGuard)
  {
    path: 'dashboard',
    component: DashboardComponent,
    canActivate: [AuthGuard],
  },
  {
    path: 'urls/new',
    component: UrlCreateComponent,
    canActivate: [AuthGuard],
  },
  {
    path: 'urls/:shortCode/stats',
    component: UrlStatsComponent,
    canActivate: [AuthGuard],
  },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule],
})
export class AppRoutingModule {}
