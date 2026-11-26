import { Component, OnInit } from '@angular/core';
import { Cash, Cashier, ROLES } from '../../core/models';
import { CashApi } from '../../core/services/api.service';
import { AuthService } from '../../core/services/auth.service';
import { NotificationService } from '../../core/services/ui.service';

const MAX_CASHIERS = 2;

@Component({
  selector: 'app-cashes',
  standalone: false,
  templateUrl: './cashes.component.html',
})
export class CashesComponent implements OnInit {
  cashes: Cash[] = [];
  availableCashiers: Cashier[] = [];
  isGestor: boolean;
  readonly maxCashiers = MAX_CASHIERS;

  showForm = false;
  editing: Cash | null = null;
  submitted = false;
  model = { cashDescription: '', active: true };

  selectedCashier: Record<number, string> = {};

  constructor(private api: CashApi, private auth: AuthService, private notifications: NotificationService) {
    this.isGestor = this.auth.hasRole(ROLES.Gestor);
  }

  ngOnInit(): void {
    this.load();
    if (this.isGestor) {
      this.api.availableCashiers().subscribe((cashiers) => (this.availableCashiers = cashiers));
    }
  }

  load(): void {
    this.api.list().subscribe((cashes) => (this.cashes = cashes));
  }

  cashiersFor(cash: Cash): Cashier[] {
    const assigned = new Set(cash.cashiers.map((cashier) => cashier.userId));
    return this.availableCashiers.filter((cashier) => !assigned.has(cashier.userId));
  }

  openCreate(): void {
    this.editing = null;
    this.submitted = false;
    this.model = { cashDescription: '', active: true };
    this.showForm = true;
  }

  openEdit(cash: Cash): void {
    this.editing = cash;
    this.submitted = false;
    this.model = { cashDescription: cash.cashDescription, active: cash.active };
    this.showForm = true;
  }

  save(): void {
    this.submitted = true;
    this.model.cashDescription = this.model.cashDescription.trim();

    if (!this.model.cashDescription) {
      return;
    }

    const request = this.editing ? this.api.update(this.editing.cashId, this.model) : this.api.create(this.model);

    request.subscribe(() => {
      this.notifications.success(this.editing ? 'Caja actualizada' : 'Caja creada');
      this.showForm = false;
      this.load();
    });
  }

  remove(cash: Cash): void {
    if (!confirm(`¿Eliminar la ${cash.cashDescription}?`)) {
      return;
    }

    this.api.delete(cash.cashId).subscribe(() => {
      this.notifications.success('Caja eliminada');
      this.load();
    });
  }

  assign(cash: Cash): void {
    const userId = Number(this.selectedCashier[cash.cashId]);

    if (!userId) {
      this.notifications.warning('Seleccione un cajero');
      return;
    }

    this.api.assignCashier(cash.cashId, userId).subscribe(() => {
      this.notifications.success('Cajero asignado a la caja');
      this.selectedCashier[cash.cashId] = '';
      this.load();
    });
  }

  unassign(cash: Cash, cashier: Cashier): void {
    if (!confirm(`¿Retirar a ${cashier.userName} de la ${cash.cashDescription}?`)) {
      return;
    }

    this.api.unassignCashier(cash.cashId, cashier.userId).subscribe(() => {
      this.notifications.success('Cajero retirado de la caja');
      this.load();
    });
  }
}
