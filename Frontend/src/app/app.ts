import { Component } from '@angular/core';
import { LoadingService, NotificationService } from './core/services/ui.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.html',
  standalone: false,
  styleUrl: './app.scss',
})
export class App {
  constructor(public loading: LoadingService, public notifications: NotificationService) {}
}
