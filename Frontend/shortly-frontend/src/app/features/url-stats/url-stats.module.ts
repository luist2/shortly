import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

// Material Modules
import { MatChipsModule } from '@angular/material/chips';

// Modules
import { SharedModule } from 'src/app/shared/shared.module';

// Components
import { UrlStatsComponent } from './url-stats.component';

const routes: Routes = [
  {
    path: '',
    component: UrlStatsComponent,
  },
];

@NgModule({
  declarations: [UrlStatsComponent],
  imports: [
    SharedModule,

    RouterModule.forChild(routes),

    // Material Modules
    MatChipsModule,
  ],
})
export class UrlStatsModule {}
