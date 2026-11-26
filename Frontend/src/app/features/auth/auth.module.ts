import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { SharedModule } from '../../shared/shared.module';
import { LoginComponent } from './login.component';
import { RecoverPasswordComponent } from './recover-password.component';

const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'login' },
  { path: 'login', component: LoginComponent },
  { path: 'recover-password', component: RecoverPasswordComponent },
];

@NgModule({
  declarations: [LoginComponent, RecoverPasswordComponent],
  imports: [SharedModule, RouterModule.forChild(routes)],
})
export class AuthModule {}
