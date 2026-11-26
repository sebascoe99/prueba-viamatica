import { Component, OnInit } from '@angular/core';
import { ATTENTION_STATUS, Cash, CatalogItem, Client, Contract, Payment, ServicePlan, Turn } from '../../core/models';
import { CashApi, CatalogApi, ClientApi, ContractApi, TurnApi } from '../../core/services/api.service';
import { AuthService } from '../../core/services/auth.service';
import { NotificationService } from '../../core/services/ui.service';

const ACTIVE_CONTRACT_STATUSES = ['VIG', 'REN'];

@Component({
  selector: 'app-cash-process',
  standalone: false,
  templateUrl: './cash-process.component.html',
})
export class CashProcessComponent implements OnInit {
  private readonly userId: number;

  myCashes: Cash[] = [];
  openCash: Cash | null = null;

  turns: Turn[] = [];
  activeTurn: Turn | null = null;

  services: ServicePlan[] = [];
  methodPayments: CatalogItem[] = [];

  contracts: Contract[] = [];
  payments: Payment[] = [];
  activeContract: Contract | null = null;

  selectedTurn: Turn | null = null;
  searchIdentification = '';
  foundClient: Client | null = null;
  searched = false;
  showClientForm = false;

  newContract = { serviceId: '', methodPaymentId: '', months: 12 };
  paymentAmount: number | null = null;
  changeServiceId = '';
  changeMethodId = '';

  readonly status = ATTENTION_STATUS;

  constructor(
    private cashApi: CashApi,
    private turnApi: TurnApi,
    private clientApi: ClientApi,
    private contractApi: ContractApi,
    private catalogs: CatalogApi,
    private auth: AuthService,
    private notifications: NotificationService
  ) {
    this.userId = this.auth.currentSession?.userId ?? 0;
  }

  ngOnInit(): void {
    this.catalogs.services().subscribe((services) => (this.services = services));
    this.catalogs.methodPayments().subscribe((methods) => (this.methodPayments = methods));
    this.loadCashes();
  }

  loadCashes(): void {
    this.cashApi.mine().subscribe((cashes) => {
      this.myCashes = cashes;
      this.openCash = cashes.find((cash) => cash.currentUserId === this.userId) ?? null;

      if (this.openCash) {
        this.loadTurns();
      } else {
        this.turns = [];
        this.activeTurn = null;
      }
    });
  }

  open(cash: Cash): void {
    this.cashApi.open(cash.cashId).subscribe(() => {
      this.notifications.success(`${cash.cashDescription} abierta`);
      this.loadCashes();
    });
  }

  close(): void {
    if (!this.openCash) {
      return;
    }

    this.cashApi.close(this.openCash.cashId).subscribe(() => {
      this.notifications.success('Caja cerrada');
      this.loadCashes();
    });
  }

  loadTurns(): void {
    if (!this.openCash) {
      return;
    }

    this.turnApi.list({ cashId: this.openCash.cashId }).subscribe((turns) => {
      this.turns = turns;
      this.activeTurn = turns.find((turn) => turn.statusId === ATTENTION_STATUS.EnAtencion && turn.cashierUserName === this.auth.currentSession?.userName) ?? null;

      if (this.activeTurn?.clientId) {
        this.loadClientData(this.activeTurn.clientId);
      }
    });
  }

  get pendingTurns(): Turn[] {
    return this.turns.filter((turn) => turn.statusId === ATTENTION_STATUS.Pendiente);
  }

  get attendedTurns(): Turn[] {
    return this.turns.filter((turn) => turn.statusId === ATTENTION_STATUS.Atendido);
  }

  selectTurn(turn: Turn): void {
    this.selectedTurn = turn;
    this.searchIdentification = '';
    this.foundClient = null;
    this.searched = false;
    this.showClientForm = false;
  }

  searchClient(): void {
    const identification = this.searchIdentification.trim();

    if (!/^[0-9]{10,13}$/.test(identification)) {
      this.notifications.warning('La identificación tiene entre 10 y 13 dígitos, solo números');
      return;
    }

    this.foundClient = null;
    this.showClientForm = false;

    this.clientApi.list({ search: identification }).subscribe((clients) => {
      this.foundClient = clients.find((client) => client.identification === identification) ?? null;
      this.searched = true;
    });
  }

  onClientRegistered(client: Client): void {
    this.foundClient = client;
    this.showClientForm = false;
  }

  startAttention(): void {
    if (!this.selectedTurn || !this.foundClient) {
      return;
    }

    this.turnApi.attend(this.selectedTurn.turnId, this.foundClient.clientId).subscribe((turn) => {
      this.notifications.success(`Atendiendo el turno ${turn.description}`);
      this.selectedTurn = null;
      this.loadTurns();
    });
  }

  loadClientData(clientId: number): void {
    this.contractApi.list(clientId).subscribe((contracts) => {
      this.contracts = contracts;
      this.activeContract = contracts.find((contract) => ACTIVE_CONTRACT_STATUSES.includes(contract.statusId)) ?? null;
      this.paymentAmount = this.activeContract?.price ?? null;
      this.changeServiceId = '';
      this.changeMethodId = '';
    });

    this.contractApi.payments(clientId).subscribe((payments) => (this.payments = payments));
  }

  get otherServices(): ServicePlan[] {
    return this.services.filter((service) => service.serviceId !== this.activeContract?.serviceId);
  }

  get otherMethods(): CatalogItem[] {
    return this.methodPayments.filter((method) => Number(method.id) !== this.activeContract?.methodPaymentId);
  }

  hire(): void {
    if (!this.activeTurn?.clientId || !this.newContract.serviceId || !this.newContract.methodPaymentId) {
      this.notifications.warning('Seleccione el servicio y la forma de pago');
      return;
    }

    const months = Number(this.newContract.months);
    if (!months || months < 1 || months > 60) {
      this.notifications.warning('La duración debe estar entre 1 y 60 meses');
      return;
    }

    this.contractApi
      .create(this.activeTurn.clientId, Number(this.newContract.serviceId), Number(this.newContract.methodPaymentId), months)
      .subscribe(() => {
        this.notifications.success('Servicio contratado');
        this.newContract = { serviceId: '', methodPaymentId: '', months: 12 };
        this.refreshClient();
      });
  }

  pay(): void {
    if (!this.activeContract) {
      return;
    }

    if (!this.paymentAmount || this.paymentAmount <= 0) {
      this.notifications.warning('Ingrese un monto mayor a cero');
      return;
    }

    this.contractApi.pay(this.activeContract.contractId, this.paymentAmount).subscribe((payment) => {
      this.notifications.success(`Pago de $${payment.amount.toFixed(2)} registrado`);
      this.refreshClient();
    });
  }

  changeService(): void {
    if (!this.activeContract || !this.changeServiceId) {
      this.notifications.warning('Seleccione el nuevo servicio');
      return;
    }

    this.contractApi.changeService(this.activeContract.contractId, Number(this.changeServiceId)).subscribe(() => {
      this.notifications.success('Servicio cambiado. El contrato anterior quedó sustituido');
      this.refreshClient();
    });
  }

  changePaymentMethod(): void {
    if (!this.activeContract || !this.changeMethodId) {
      this.notifications.warning('Seleccione la nueva forma de pago');
      return;
    }

    this.contractApi.changePaymentMethod(this.activeContract.contractId, Number(this.changeMethodId)).subscribe(() => {
      this.notifications.success('Forma de pago actualizada');
      this.refreshClient();
    });
  }

  cancelContract(): void {
    if (!this.activeContract || !confirm('¿Cancelar el contrato del cliente?')) {
      return;
    }

    this.contractApi.cancel(this.activeContract.contractId).subscribe(() => {
      this.notifications.success('Contrato cancelado');
      this.refreshClient();
    });
  }

  finish(): void {
    if (!this.activeTurn) {
      return;
    }

    this.turnApi.finish(this.activeTurn.turnId).subscribe((turn) => {
      this.notifications.success(`Turno ${turn.description} finalizado`);
      this.contracts = [];
      this.payments = [];
      this.activeContract = null;
      this.loadTurns();
    });
  }

  contractBadge(statusId: string): string {
    switch (statusId) {
      case 'VIG':
      case 'REN':
        return 'text-bg-success';
      case 'SUS':
        return 'text-bg-secondary';
      default:
        return 'text-bg-danger';
    }
  }

  private refreshClient(): void {
    if (this.activeTurn?.clientId) {
      this.loadClientData(this.activeTurn.clientId);
    }
  }
}
