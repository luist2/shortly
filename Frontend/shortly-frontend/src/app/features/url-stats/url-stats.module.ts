import { RouterModule, Routes } from '@angular/router';
import { UrlStatsComponent } from './url-stats.component';
import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatChipsModule } from '@angular/material/chips';

const routes: Routes = [
  {
    path: '',
    component: UrlStatsComponent,
  },
];

@NgModule({
  declarations: [UrlStatsComponent],
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterModule.forChild(routes),

    // Material Modules
    MatProgressSpinnerModule,
    MatCardModule,
    MatIconModule,
    MatChipsModule,
  ],
})
export class UrlStatsModule {}
