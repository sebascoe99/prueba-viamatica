import { Component, OnInit } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { BulkResult, CatalogItem, ROLES, User } from '../../core/models';
import { CatalogApi, UserApi } from '../../core/services/api.service';
import { AuthService } from '../../core/services/auth.service';
import { NotificationService } from '../../core/services/ui.service';
import { BulkColumn } from '../../shared/bulk-upload.component';

const USERNAME_PATTERN = /^(?=.*[A-Za-z])(?=.*[0-9])[A-Za-z0-9]{8,20}$/;
const PASSWORD_PATTERN = /^(?=.*[0-9])(?=.*[A-Z]).{8,30}$/;
const IDENTIFICATION_PATTERN = /^[0-9]{10,13}$/;

@Component({
  selector: 'app-users',
  standalone: false,
  templateUrl: './users.component.html',
})
export class UsersComponent implements OnInit {
  users: User[] = [];
  statuses: CatalogItem[] = [];
  isAdmin: boolean;

  readonly roles = [
    { id: ROLES.Gestor, description: 'Gestor' },
    { id: ROLES.Cajero, description: 'Cajero' },
  ];

  filters = { rolId: '', statusId: '', search: '' };

  showForm = false;
  editing: User | null = null;
  submitted = false;
  form;

  showBulk = false;
  bulkResult: BulkResult | null = null;
  readonly bulkColumns: BulkColumn[] = [
    { key: 'userName', label: 'Usuario', example: 'cajero2030' },
    { key: 'password', label: 'Contraseña', example: 'Cajero2030' },
    { key: 'firstName', label: 'Nombres', example: 'María' },
    { key: 'lastName', label: 'Apellidos', example: 'López' },
    { key: 'email', label: 'Correo', example: 'maria@correo.com' },
    { key: 'identification', label: 'Identificación', example: '0912345678' },
    { key: 'rol', label: 'Rol', example: 'Cajero' },
  ];

  constructor(
    private fb: FormBuilder,
    private api: UserApi,
    private catalogs: CatalogApi,
    private auth: AuthService,
    private notifications: NotificationService
  ) {
    this.isAdmin = this.auth.hasRole(ROLES.Administrador);

    this.form = this.fb.nonNullable.group({
      userName: ['', [Validators.required, Validators.pattern(USERNAME_PATTERN)]],
      password: ['', [Validators.required, Validators.pattern(PASSWORD_PATTERN)]],
      firstName: ['', [Validators.required, Validators.maxLength(50)]],
      lastName: ['', [Validators.required, Validators.maxLength(50)]],
      email: ['', [Validators.required, Validators.email, Validators.maxLength(100)]],
      identification: ['', [Validators.required, Validators.pattern(IDENTIFICATION_PATTERN)]],
      rolId: [ROLES.Cajero as number, [Validators.required]],
    });
  }

  get f() {
    return this.form.controls;
  }

  ngOnInit(): void {
    this.catalogs.userStatuses().subscribe((statuses) => (this.statuses = statuses));
    this.load();
  }

  load(): void {
    this.api.list(this.filters).subscribe((users) => (this.users = users));
  }

  clearFilters(): void {
    this.filters = { rolId: '', statusId: '', search: '' };
    this.load();
  }

  canManage(user: User): boolean {
    return this.isAdmin && user.rolId !== ROLES.Administrador;
  }

  openCreate(): void {
    this.editing = null;
    this.submitted = false;
    this.form.reset({ rolId: ROLES.Cajero });
    this.f.userName.enable();
    this.f.password.enable();
    this.showForm = true;
  }

  openEdit(user: User): void {
    this.editing = user;
    this.submitted = false;
    this.form.reset({
      userName: user.userName,
      firstName: user.firstName,
      lastName: user.lastName,
      email: user.email,
      identification: user.identification,
      rolId: user.rolId,
    });
    this.f.userName.disable();
    this.f.password.disable();
    this.showForm = true;
  }

  save(): void {
    this.submitted = true;

    if (this.form.invalid) {
      return;
    }

    const value = this.form.getRawValue();
    const rolId = Number(value.rolId);

    if (this.editing) {
      const { firstName, lastName, email, identification } = value;
      this.api.update(this.editing.userId, { firstName, lastName, email, identification, rolId }).subscribe(() => {
        this.notifications.success('Usuario actualizado');
        this.afterSave();
      });
      return;
    }

    this.api.create({ ...value, rolId }).subscribe((user) => {
      const message = user.statusId === 'PEN' ? 'Usuario creado. Queda pendiente de aprobación por un administrador' : 'Usuario creado';
      this.notifications.success(message);
      this.afterSave();
    });
  }

  changeStatus(user: User, statusId: string): void {
    if (statusId === user.statusId) {
      return;
    }

    this.api.changeStatus(user.userId, statusId).subscribe({
      next: () => {
        this.notifications.success(`Estado de ${user.userName} actualizado`);
        this.load();
      },
      error: () => this.load(),
    });
  }

  approve(user: User): void {
    this.api.approve(user.userId).subscribe(() => {
      this.notifications.success(`El usuario ${user.userName} fue aprobado`);
      this.load();
    });
  }

  remove(user: User): void {
    if (!confirm(`¿Eliminar al usuario ${user.userName}?`)) {
      return;
    }

    this.api.delete(user.userId).subscribe(() => {
      this.notifications.success('Usuario eliminado');
      this.load();
    });
  }

  openBulk(): void {
    this.bulkResult = null;
    this.showBulk = true;
  }

  uploadBulk(rows: Record<string, string>[]): void {
    const payload = rows.map((row) => ({ ...row, rolId: this.toRolId(row['rol']) }));

    this.api.bulk(payload).subscribe((result) => {
      this.bulkResult = result;
      this.notifications.success(`Carga finalizada: ${result.created} creados, ${result.failed} rechazados`);
      this.load();
    });
  }

  statusBadge(statusId: string): string {
    switch (statusId) {
      case 'ACT':
        return 'text-bg-success';
      case 'PEN':
        return 'text-bg-warning';
      case 'BLO':
        return 'text-bg-danger';
      default:
        return 'text-bg-secondary';
    }
  }

  private afterSave(): void {
    this.showForm = false;
    this.load();
  }

  private toRolId(value: string): number {
    const text = (value ?? '').trim().toLowerCase();
    if (text === 'gestor' || text === '2') {
      return ROLES.Gestor;
    }
    if (text === 'cajero' || text === '3') {
      return ROLES.Cajero;
    }
    return 0;
  }
}
