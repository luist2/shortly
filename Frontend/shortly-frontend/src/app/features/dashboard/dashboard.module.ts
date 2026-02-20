import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { FormsModule } from '@angular/forms';

// Material Modules - Específicos de Dashboard
import { MatChipsModule } from '@angular/material/chips';
import { MatSortModule } from '@angular/material/sort';
import { MatTableModule } from '@angular/material/table';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatProgressBarModule } from '@angular/material/progress-bar';

// Modules
import { SharedModule } from 'src/app/shared/shared.module';

// Components
import { DashboardComponent } from './dashboard.component';

const routes: Routes = [
  {
    path: '',
    component: DashboardComponent,
  },
];

@NgModule({
  declarations: [DashboardComponent],
  imports: [
    SharedModule,

    RouterModule.forChild(routes),
    FormsModule,

    // Material Modules
    MatChipsModule,
    MatSortModule,
    MatTableModule,
    MatButtonToggleModule,
    MatPaginatorModule,
    MatProgressBarModule,
  ],
})
export class DashboardModule {}
