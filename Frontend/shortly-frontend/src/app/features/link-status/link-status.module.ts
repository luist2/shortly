import { NgModule } from '@angular/core';
import { SharedModule } from 'src/app/shared/shared.module';
import { LinkStatusComponent } from './link-status.component';

@NgModule({
  declarations: [LinkStatusComponent],
  imports: [SharedModule],
  exports: [LinkStatusComponent],
})
export class LinkStatusModule {}
