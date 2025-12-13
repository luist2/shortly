import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Routes } from '@angular/router';

// Material Modules
import { MatTableModule } from '@angular/material/table';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { ClipboardModule } from '@angular/cdk/clipboard';
import { SharedModule } from 'src/app/shared/shared.module';
import { MatChipsModule } from '@angular/material/chips';

// Components
import { DashboardComponent } from './dashboard.component';
import { FormsModule } from '@angular/forms';
import { MatSortModule } from '@angular/material/sort';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';

const routes: Routes = [
  {
    path: '',
    component: DashboardComponent,
  },
];

@NgModule({
  declarations: [DashboardComponent],
  imports: [
    CommonModule,
    RouterModule.forChild(routes),
    FormsModule,

    // Material Modules
    MatTableModule,
    MatProgressSpinnerModule,
    MatCardModule,
    MatIconModule,
    MatButtonModule,
    MatTooltipModule,
    MatSnackBarModule,
    MatSortModule,
    MatFormFieldModule,
    MatInputModule,
    MatChipsModule,

    // CDK
    ClipboardModule,
    //Shared Module
    SharedModule,
  ],
})
export class DashboardModule {}
