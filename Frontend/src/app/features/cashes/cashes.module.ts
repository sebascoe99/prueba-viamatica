import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { SharedModule } from '../../shared/shared.module';
import { CashesComponent } from './cashes.component';

@NgModule({
  declarations: [CashesComponent],
  imports: [SharedModule, RouterModule.forChild([{ path: '', component: CashesComponent }])],
})
export class CashesModule {}
