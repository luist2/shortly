import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AuthGuard } from './core/guards/auth.guard';
import { PublicGuard } from './core/guards/public.guard';

const routes: Routes = [
  // Ruta raíz - Redirige según autenticación
  {
    path: '',
    redirectTo: '/dashboard',
    pathMatch: 'full',
  },

  // RUTAS PÚBLICAS (con PublicGuard)

  // RUTAS PROTEGIDAS (con AuthGuard)
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule],
})
export class AppRoutingModule {}
