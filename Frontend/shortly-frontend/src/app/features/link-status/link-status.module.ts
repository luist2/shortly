import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { SharedModule } from 'src/app/shared/shared.module';
import { LinkStatusComponent } from './link-status.component';

@NgModule({
  declarations: [LinkStatusComponent],
  imports: [SharedModule, RouterModule],
  exports: [LinkStatusComponent],
})
export class LinkStatusModule {}
