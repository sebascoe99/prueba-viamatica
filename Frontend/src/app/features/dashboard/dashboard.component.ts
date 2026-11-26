import { Component, OnInit } from '@angular/core';
import { Indicators } from '../../core/models';
import { DashboardApi } from '../../core/services/api.service';
import { NotificationService } from '../../core/services/ui.service';

function today(): string {
  const now = new Date();
  const month = String(now.getMonth() + 1).padStart(2, '0');
  const day = String(now.getDate()).padStart(2, '0');
  return `${now.getFullYear()}-${month}-${day}`;
}

@Component({
  selector: 'app-dashboard',
  standalone: false,
  templateUrl: './dashboard.component.html',
})
export class DashboardComponent implements OnInit {
  from = today();
  to = today();
  data: Indicators | null = null;

  constructor(private dashboard: DashboardApi, private notifications: NotificationService) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    if (!this.from || !this.to) {
      this.notifications.warning('Seleccione la fecha desde y la fecha hasta');
      return;
    }

    if (this.from > this.to) {
      this.notifications.warning('La fecha desde no puede ser mayor que la fecha hasta');
      return;
    }

    this.dashboard.indicators(this.from, this.to).subscribe((data) => (this.data = data));
  }
}
