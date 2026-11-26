import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { SharedModule } from '../../shared/shared.module';
import { TurnsComponent } from './turns.component';

@NgModule({
  declarations: [TurnsComponent],
  imports: [SharedModule, RouterModule.forChild([{ path: '', component: TurnsComponent }])],
})
export class TurnsModule {}
