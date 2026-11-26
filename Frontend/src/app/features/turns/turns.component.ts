import { Component, OnInit } from '@angular/core';
import { ATTENTION_STATUS, Cash, CatalogItem, Turn } from '../../core/models';
import { CashApi, CatalogApi, TurnApi } from '../../core/services/api.service';
import { NotificationService } from '../../core/services/ui.service';

@Component({
  selector: 'app-turns',
  standalone: false,
  templateUrl: './turns.component.html',
})
export class TurnsComponent implements OnInit {
  cashes: Cash[] = [];
  attentionTypes: CatalogItem[] = [];
  turns: Turn[] = [];

  submitted = false;
  model = { cashId: '', attentionTypeId: '' };
  filterCashId = '';

  reassigning: Turn | null = null;
  reassignCashId = '';

  readonly status = ATTENTION_STATUS;

  constructor(
    private api: TurnApi,
    private cashApi: CashApi,
    private catalogs: CatalogApi,
    private notifications: NotificationService
  ) {}

  ngOnInit(): void {
    this.cashApi.list().subscribe((cashes) => (this.cashes = cashes.filter((cash) => cash.active)));
    this.catalogs.attentionTypes().subscribe((types) => (this.attentionTypes = types));
    this.load();
  }

  load(): void {
    this.api.list({ cashId: this.filterCashId }).subscribe((turns) => (this.turns = turns));
  }

  get pendingCount(): number {
    return this.turns.filter((turn) => turn.statusId === ATTENTION_STATUS.Pendiente).length;
  }

  get attendedCount(): number {
    return this.turns.filter((turn) => turn.statusId === ATTENTION_STATUS.Atendido).length;
  }

  create(): void {
    this.submitted = true;

    if (!this.model.cashId || !this.model.attentionTypeId) {
      return;
    }

    this.api.create(Number(this.model.cashId), this.model.attentionTypeId).subscribe((turn) => {
      this.notifications.success(`Turno ${turn.description} asignado a ${turn.cashDescription}`);
      this.submitted = false;
      this.load();
    });
  }

  openReassign(turn: Turn): void {
    this.reassigning = turn;
    this.reassignCashId = '';
  }

  reassign(): void {
    if (!this.reassigning || !this.reassignCashId) {
      this.notifications.warning('Seleccione la caja de destino');
      return;
    }

    this.api.reassign(this.reassigning.turnId, Number(this.reassignCashId)).subscribe((turn) => {
      this.notifications.success(`Turno ${turn.description} reasignado a ${turn.cashDescription}`);
      this.reassigning = null;
      this.load();
    });
  }

  remove(turn: Turn): void {
    if (!confirm(`¿Eliminar el turno ${turn.description}?`)) {
      return;
    }

    this.api.delete(turn.turnId).subscribe(() => {
      this.notifications.success('Turno eliminado');
      this.load();
    });
  }

  statusBadge(statusId: number): string {
    switch (statusId) {
      case ATTENTION_STATUS.Pendiente:
        return 'text-bg-warning';
      case ATTENTION_STATUS.EnAtencion:
        return 'text-bg-info';
      case ATTENTION_STATUS.Atendido:
        return 'text-bg-success';
      default:
        return 'text-bg-secondary';
    }
  }
}
