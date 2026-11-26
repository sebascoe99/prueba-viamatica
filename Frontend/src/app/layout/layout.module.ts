import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { roleGuard } from '../core/guards/auth.guard';
import { ROLES } from '../core/models';
import { SharedModule } from '../shared/shared.module';
import { LayoutComponent } from './layout.component';

const routes: Routes = [
  {
    path: '',
    component: LayoutComponent,
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'home' },
      {
        path: 'home',
        loadChildren: () => import('../features/home/home.module').then((m) => m.HomeModule),
      },
      {
        path: 'dashboard',
        canActivate: [roleGuard],
        data: { roles: [ROLES.Administrador] },
        loadChildren: () => import('../features/dashboard/dashboard.module').then((m) => m.DashboardModule),
      },
      {
        path: 'users',
        canActivate: [roleGuard],
        data: { roles: [ROLES.Administrador, ROLES.Gestor] },
        loadChildren: () => import('../features/users/users.module').then((m) => m.UsersModule),
      },
      {
        path: 'cashes',
        canActivate: [roleGuard],
        data: { roles: [ROLES.Administrador, ROLES.Gestor] },
        loadChildren: () => import('../features/cashes/cashes.module').then((m) => m.CashesModule),
      },
      {
        path: 'services',
        canActivate: [roleGuard],
        data: { roles: [ROLES.Administrador] },
        loadChildren: () => import('../features/services/services.module').then((m) => m.ServicesModule),
      },
      {
        path: 'turns',
        canActivate: [roleGuard],
        data: { roles: [ROLES.Gestor] },
        loadChildren: () => import('../features/turns/turns.module').then((m) => m.TurnsModule),
      },
      {
        path: 'cash-process',
        canActivate: [roleGuard],
        data: { roles: [ROLES.Cajero] },
        loadChildren: () => import('../features/cash-process/cash-process.module').then((m) => m.CashProcessModule),
      },
      {
        path: 'clients',
        canActivate: [roleGuard],
        data: { roles: [ROLES.Cajero] },
        loadChildren: () => import('../features/clients/clients.module').then((m) => m.ClientsModule),
      },
    ],
  },
];

@NgModule({
  declarations: [LayoutComponent],
  imports: [SharedModule, RouterModule.forChild(routes)],
})
export class LayoutModule {}
