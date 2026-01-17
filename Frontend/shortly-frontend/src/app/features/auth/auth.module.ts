import { NgModule } from '@angular/core';
import { LoginComponent } from './login/login.component';
import { ReactiveFormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';

// Modules
import { SharedModule } from 'src/app/shared/shared.module';

import { RegisterComponent } from './register/register.component';

@NgModule({
  declarations: [LoginComponent, RegisterComponent],
  imports: [
    SharedModule,
    ReactiveFormsModule,
    RouterModule,
  ],
  exports: [],
})
export class AuthModule {}
