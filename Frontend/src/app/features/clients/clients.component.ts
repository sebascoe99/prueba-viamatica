import { Component, OnInit } from '@angular/core';
import { BulkResult, Client } from '../../core/models';
import { ClientApi } from '../../core/services/api.service';
import { NotificationService } from '../../core/services/ui.service';
import { BulkColumn } from '../../shared/bulk-upload.component';

@Component({
  selector: 'app-clients',
  standalone: false,
  templateUrl: './clients.component.html',
})
export class ClientsComponent implements OnInit {
  clients: Client[] = [];
  filters = { search: '', active: '' };

  showForm = false;
  editing: Client | null = null;

  showBulk = false;
  bulkResult: BulkResult | null = null;
  readonly bulkColumns: BulkColumn[] = [
    { key: 'name', label: 'Nombres', example: 'Luis' },
    { key: 'lastName', label: 'Apellidos', example: 'Paz' },
    { key: 'identification', label: 'Identificación', example: '0912345678' },
    { key: 'email', label: 'Correo', example: 'luis@correo.com' },
    { key: 'phoneNumber', label: 'Teléfono', example: '0991234567' },
    { key: 'address', label: 'Dirección', example: 'Av. 9 de Octubre 1234 y Malecón' },
    { key: 'referenceAddress', label: 'Referencia', example: 'Frente al edificio de la gobernación' },
  ];

  constructor(private api: ClientApi, private notifications: NotificationService) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.api.list(this.filters).subscribe((clients) => (this.clients = clients));
  }

  clearFilters(): void {
    this.filters = { search: '', active: '' };
    this.load();
  }

  openCreate(): void {
    this.editing = null;
    this.showForm = true;
  }

  openEdit(client: Client): void {
    this.editing = client;
    this.showForm = true;
  }

  onSaved(): void {
    this.showForm = false;
    this.load();
  }

  toggleStatus(client: Client): void {
    this.api.changeStatus(client.clientId, !client.active).subscribe(() => {
      this.notifications.success(`Cliente ${client.active ? 'inactivado' : 'activado'}`);
      this.load();
    });
  }

  remove(client: Client): void {
    if (!confirm(`¿Eliminar al cliente ${client.name} ${client.lastName}?`)) {
      return;
    }

    this.api.delete(client.clientId).subscribe(() => {
      this.notifications.success('Cliente eliminado');
      this.load();
    });
  }

  openBulk(): void {
    this.bulkResult = null;
    this.showBulk = true;
  }

  uploadBulk(rows: Record<string, string>[]): void {
    this.api.bulk(rows).subscribe((result) => {
      this.bulkResult = result;
      this.notifications.success(`Carga finalizada: ${result.created} creados, ${result.failed} rechazados`);
      this.load();
    });
  }
}
