import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { SharedModule } from '../../shared/shared.module';
import { ClientFormModule } from '../clients/client-form.module';
import { CashProcessComponent } from './cash-process.component';

@NgModule({
  declarations: [CashProcessComponent],
  imports: [SharedModule, ClientFormModule, RouterModule.forChild([{ path: '', component: CashProcessComponent }])],
})
export class CashProcessModule {}
