import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { RouterModule, Routes } from '@angular/router';

// Material Modules
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';

// Modules
import { SharedModule } from 'src/app/shared/shared.module';

// Components
import { UrlCreateComponent } from './url-create.component';

const routes: Routes = [
  {
    path: '',
    component: UrlCreateComponent,
  },
];

@NgModule({
  declarations: [UrlCreateComponent],
  imports: [
    SharedModule,

    ReactiveFormsModule,
    RouterModule.forChild(routes),

    // Material Modules
    MatFormFieldModule,
    MatInputModule,
  ],
})
export class UrlCreateModule {}
