import { NgModule } from '@angular/core';
import { SharedModule } from '../../shared/shared.module';
import { ClientFormComponent } from './client-form.component';

@NgModule({
  declarations: [ClientFormComponent],
  imports: [SharedModule],
  exports: [ClientFormComponent],
})
export class ClientFormModule {}
