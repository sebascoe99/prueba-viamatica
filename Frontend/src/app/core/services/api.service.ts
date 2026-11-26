import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import {
  BulkResult,
  Cash,
  Cashier,
  CatalogItem,
  Client,
  Contract,
  Indicators,
  MenuItem,
  Payment,
  ServicePlan,
  Turn,
  User,
  Welcome,
} from '../models';

function toParams(values: Record<string, unknown>): HttpParams {
  let params = new HttpParams();
  for (const [key, value] of Object.entries(values)) {
    if (value !== null && value !== undefined && value !== '') {
      params = params.set(key, String(value));
    }
  }
  return params;
}

@Injectable({ providedIn: 'root' })
export class CatalogApi {
  constructor(private http: HttpClient) {}

  menu(): Observable<MenuItem[]> {
    return this.http.get<MenuItem[]>('/api/menu');
  }

  roles(): Observable<CatalogItem[]> {
    return this.http.get<CatalogItem[]>('/api/catalogs/roles');
  }

  userStatuses(): Observable<CatalogItem[]> {
    return this.http.get<CatalogItem[]>('/api/catalogs/user-statuses');
  }

  attentionTypes(): Observable<CatalogItem[]> {
    return this.http.get<CatalogItem[]>('/api/catalogs/attention-types');
  }

  methodPayments(): Observable<CatalogItem[]> {
    return this.http.get<CatalogItem[]>('/api/catalogs/method-payments');
  }

  services(): Observable<ServicePlan[]> {
    return this.http.get<ServicePlan[]>('/api/catalogs/services');
  }
}

@Injectable({ providedIn: 'root' })
export class DashboardApi {
  constructor(private http: HttpClient) {}

  welcome(): Observable<Welcome> {
    return this.http.get<Welcome>('/api/dashboard/welcome');
  }

  indicators(from: string, to: string): Observable<Indicators> {
    return this.http.get<Indicators>('/api/dashboard/indicators', { params: toParams({ from, to }) });
  }
}

@Injectable({ providedIn: 'root' })
export class UserApi {
  private readonly url = '/api/users';

  constructor(private http: HttpClient) {}

  list(filters: { rolId?: number | string; statusId?: string; search?: string }): Observable<User[]> {
    return this.http.get<User[]>(this.url, { params: toParams(filters) });
  }

  create(user: unknown): Observable<User> {
    return this.http.post<User>(this.url, user);
  }

  bulk(rows: unknown[]): Observable<BulkResult> {
    return this.http.post<BulkResult>(`${this.url}/bulk`, rows);
  }

  update(userId: number, user: unknown): Observable<User> {
    return this.http.put<User>(`${this.url}/${userId}`, user);
  }

  changeStatus(userId: number, statusId: string): Observable<User> {
    return this.http.patch<User>(`${this.url}/${userId}/status`, { statusId });
  }

  approve(userId: number): Observable<User> {
    return this.http.post<User>(`${this.url}/${userId}/approve`, {});
  }

  delete(userId: number): Observable<void> {
    return this.http.delete<void>(`${this.url}/${userId}`);
  }
}

@Injectable({ providedIn: 'root' })
export class ClientApi {
  private readonly url = '/api/clients';

  constructor(private http: HttpClient) {}

  list(filters: { search?: string; active?: boolean | string }): Observable<Client[]> {
    return this.http.get<Client[]>(this.url, { params: toParams(filters) });
  }

  byIdentification(identification: string): Observable<Client> {
    return this.http.get<Client>(`${this.url}/by-identification/${identification}`);
  }

  create(client: unknown): Observable<Client> {
    return this.http.post<Client>(this.url, client);
  }

  bulk(rows: unknown[]): Observable<BulkResult> {
    return this.http.post<BulkResult>(`${this.url}/bulk`, rows);
  }

  update(clientId: number, client: unknown): Observable<Client> {
    return this.http.put<Client>(`${this.url}/${clientId}`, client);
  }

  changeStatus(clientId: number, active: boolean): Observable<Client> {
    return this.http.patch<Client>(`${this.url}/${clientId}/status`, { active });
  }

  delete(clientId: number): Observable<void> {
    return this.http.delete<void>(`${this.url}/${clientId}`);
  }
}

@Injectable({ providedIn: 'root' })
export class CashApi {
  private readonly url = '/api/cashes';

  constructor(private http: HttpClient) {}

  list(): Observable<Cash[]> {
    return this.http.get<Cash[]>(this.url);
  }

  create(cash: unknown): Observable<Cash> {
    return this.http.post<Cash>(this.url, cash);
  }

  update(cashId: number, cash: unknown): Observable<Cash> {
    return this.http.put<Cash>(`${this.url}/${cashId}`, cash);
  }

  delete(cashId: number): Observable<void> {
    return this.http.delete<void>(`${this.url}/${cashId}`);
  }

  availableCashiers(): Observable<Cashier[]> {
    return this.http.get<Cashier[]>(`${this.url}/available-cashiers`);
  }

  assignCashier(cashId: number, userId: number): Observable<Cash> {
    return this.http.post<Cash>(`${this.url}/${cashId}/cashiers`, { userId });
  }

  unassignCashier(cashId: number, userId: number): Observable<Cash> {
    return this.http.delete<Cash>(`${this.url}/${cashId}/cashiers/${userId}`);
  }

  mine(): Observable<Cash[]> {
    return this.http.get<Cash[]>(`${this.url}/mine`);
  }

  open(cashId: number): Observable<Cash> {
    return this.http.post<Cash>(`${this.url}/${cashId}/open`, {});
  }

  close(cashId: number): Observable<Cash> {
    return this.http.post<Cash>(`${this.url}/${cashId}/close`, {});
  }
}

@Injectable({ providedIn: 'root' })
export class TurnApi {
  private readonly url = '/api/turns';

  constructor(private http: HttpClient) {}

  list(filters: { cashId?: number | string; date?: string; statusId?: number | string }): Observable<Turn[]> {
    return this.http.get<Turn[]>(this.url, { params: toParams(filters) });
  }

  create(cashId: number, attentionTypeId: string): Observable<Turn> {
    return this.http.post<Turn>(this.url, { cashId, attentionTypeId });
  }

  reassign(turnId: number, cashId: number): Observable<Turn> {
    return this.http.put<Turn>(`${this.url}/${turnId}`, { cashId });
  }

  delete(turnId: number): Observable<void> {
    return this.http.delete<void>(`${this.url}/${turnId}`);
  }

  attend(turnId: number, clientId: number): Observable<Turn> {
    return this.http.post<Turn>(`${this.url}/${turnId}/attend`, { clientId });
  }

  finish(turnId: number): Observable<Turn> {
    return this.http.post<Turn>(`${this.url}/${turnId}/finish`, {});
  }
}

@Injectable({ providedIn: 'root' })
export class ContractApi {
  private readonly url = '/api/contracts';

  constructor(private http: HttpClient) {}

  list(clientId: number): Observable<Contract[]> {
    return this.http.get<Contract[]>(this.url, { params: toParams({ clientId }) });
  }

  create(clientId: number, serviceId: number, methodPaymentId: number, months: number): Observable<Contract> {
    return this.http.post<Contract>(this.url, { clientId, serviceId, methodPaymentId, months });
  }

  changeService(contractId: number, serviceId: number): Observable<Contract> {
    return this.http.post<Contract>(`${this.url}/${contractId}/change-service`, { serviceId });
  }

  changePaymentMethod(contractId: number, methodPaymentId: number): Observable<Contract> {
    return this.http.post<Contract>(`${this.url}/${contractId}/change-payment-method`, { methodPaymentId });
  }

  cancel(contractId: number): Observable<Contract> {
    return this.http.post<Contract>(`${this.url}/${contractId}/cancel`, {});
  }

  payments(clientId: number): Observable<Payment[]> {
    return this.http.get<Payment[]>('/api/payments', { params: toParams({ clientId }) });
  }

  pay(contractId: number, amount: number | null): Observable<Payment> {
    return this.http.post<Payment>('/api/payments', { contractId, amount });
  }
}
