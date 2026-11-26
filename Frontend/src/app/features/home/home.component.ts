import { Component, OnInit } from '@angular/core';
import { ROLES, Welcome } from '../../core/models';
import { DashboardApi, UserApi } from '../../core/services/api.service';
import { AuthService } from '../../core/services/auth.service';
import { NotificationService } from '../../core/services/ui.service';

@Component({
  selector: 'app-home',
  standalone: false,
  templateUrl: './home.component.html',
})
export class HomeComponent implements OnInit {
  welcome: Welcome | null = null;
  isAdmin: boolean;
  showPending: boolean;

  constructor(
    private dashboard: DashboardApi,
    private users: UserApi,
    private auth: AuthService,
    private notifications: NotificationService
  ) {
    this.isAdmin = this.auth.hasRole(ROLES.Administrador);
    this.showPending = this.auth.hasRole(ROLES.Administrador, ROLES.Gestor);
  }

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.dashboard.welcome().subscribe((welcome) => (this.welcome = welcome));
  }

  approve(userId: number, userName: string): void {
    this.users.approve(userId).subscribe(() => {
      this.notifications.success(`El usuario ${userName} fue aprobado`);
      this.load();
    });
  }
}
