import { Component, OnInit } from '@angular/core';
import { MenuItem, Session } from '../core/models';
import { CatalogApi } from '../core/services/api.service';
import { AuthService } from '../core/services/auth.service';

@Component({
  selector: 'app-layout',
  standalone: false,
  templateUrl: './layout.component.html',
  styleUrl: './layout.component.scss',
})
export class LayoutComponent implements OnInit {
  menu: MenuItem[] = [];
  session: Session | null;
  sidebarOpen = false;

  constructor(private auth: AuthService, private catalogs: CatalogApi) {
    this.session = this.auth.currentSession;
  }

  ngOnInit(): void {
    this.catalogs.menu().subscribe((menu) => (this.menu = menu));
  }

  logout(): void {
    this.auth.logout();
  }
}
