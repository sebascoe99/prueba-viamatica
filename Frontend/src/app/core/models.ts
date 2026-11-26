export const ROLES = { Administrador: 1, Gestor: 2, Cajero: 3 } as const;

export const ATTENTION_STATUS = { Pendiente: 1, EnAtencion: 2, Atendido: 3, Cancelado: 4 } as const;

export interface Session {
  token: string;
  expiresAt: string;
  userId: number;
  userName: string;
  fullName: string;
  email: string;
  rolId: number;
  rolName: string;
}

export interface MenuItem {
  menuId: number;
  name: string;
  route: string;
  icon?: string;
}

export interface CatalogItem {
  id: string;
  description: string;
}

export interface User {
  userId: number;
  userName: string;
  firstName: string;
  lastName: string;
  email: string;
  identification: string;
  rolId: number;
  rolName: string;
  statusId: string;
  statusDescription: string;
  sessionActive: boolean;
  creationDate: string;
  userCreate?: number;
  userApproval?: number;
  dateApproval?: string;
  lastLogin?: string;
}

export interface Client {
  clientId: number;
  name: string;
  lastName: string;
  identification: string;
  email: string;
  phoneNumber: string;
  address: string;
  referenceAddress: string;
  active: boolean;
  createdAt: string;
}

export interface Cashier {
  userId: number;
  userName: string;
  fullName: string;
}

export interface Cash {
  cashId: number;
  cashDescription: string;
  active: boolean;
  currentUserId?: number;
  currentUserName?: string;
  openedAt?: string;
  cashiers: Cashier[];
}

export interface Turn {
  turnId: number;
  description: string;
  date: string;
  cashId: number;
  cashDescription: string;
  attentionTypeId: string;
  attentionTypeDescription: string;
  userGestorId: number;
  gestorUserName: string;
  statusId: number;
  statusDescription: string;
  attentionId?: number;
  clientId?: number;
  clientName?: string;
  clientIdentification?: string;
  cashierUserName?: string;
  attendedAt?: string;
}

export interface ServicePlan {
  serviceId: number;
  serviceName: string;
  serviceDescription: string;
  speedMbps: number;
  price: number;
  devices: string[];
}

export interface Contract {
  contractId: number;
  startDate: string;
  endDate: string;
  clientId: number;
  clientName: string;
  clientIdentification: string;
  serviceId: number;
  serviceName: string;
  speedMbps: number;
  price: number;
  statusId: string;
  statusDescription: string;
  methodPaymentId: number;
  methodPaymentDescription: string;
  previousContractId?: number;
}

export interface Payment {
  paymentId: number;
  paymentDate: string;
  amount: number;
  contractId: number;
  serviceName: string;
  clientId: number;
  clientName: string;
  methodPaymentId: number;
  methodPaymentDescription: string;
  cashierUserName: string;
}

export interface BulkRowResult {
  row: number;
  key: string;
  success: boolean;
  message: string;
}

export interface BulkResult {
  total: number;
  created: number;
  failed: number;
  rows: BulkRowResult[];
}

export interface PendingUser {
  userId: number;
  userName: string;
  fullName: string;
  rolName: string;
  creationDate: string;
}

export interface Welcome {
  userId: number;
  userName: string;
  fullName: string;
  email: string;
  rolName: string;
  lastLogin?: string;
  turnsAttendedToday: number;
  turnsAttendedScope: string;
  pendingUsers: PendingUser[];
}

export interface Indicators {
  from: string;
  to: string;
  users: {
    total: number;
    active: number;
    inactive: number;
    blocked: number;
    pendingApproval: number;
    sessionActive: number;
    sessionInactive: number;
  };
  cashes: {
    cashId: number;
    cashDescription: string;
    active: boolean;
    operatedBy?: string;
    turnsAssigned: number;
    turnsAttended: number;
    turnsPending: number;
  }[];
  cashiers: {
    userId: number;
    userName: string;
    fullName: string;
    turnsAttended: number;
    paymentsCount: number;
    paymentsTotal: number;
  }[];
  gestores: {
    userId: number;
    userName: string;
    fullName: string;
    turnsAssigned: number;
    usersCreated: number;
  }[];
}
