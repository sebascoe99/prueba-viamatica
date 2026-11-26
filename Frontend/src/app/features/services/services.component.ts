import { Component, OnInit } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { ServicePlan } from '../../core/models';
import { ServiceAdminApi } from '../../core/services/api.service';
import { NotificationService } from '../../core/services/ui.service';

@Component({
  selector: 'app-services',
  standalone: false,
  templateUrl: './services.component.html',
})
export class ServicesComponent implements OnInit {
  services: ServicePlan[] = [];

  showForm = false;
  editing: ServicePlan | null = null;
  submitted = false;
  form;

  constructor(private fb: FormBuilder, private api: ServiceAdminApi, private notifications: NotificationService) {
    this.form = this.fb.nonNullable.group({
      serviceName: ['', [Validators.required, Validators.maxLength(100)]],
      serviceDescription: ['', [Validators.required, Validators.maxLength(150)]],
      speedMbps: [100, [Validators.required, Validators.min(1)]],
      price: [0, [Validators.required, Validators.min(0)]],
      active: [true],
      devices: [''],
    });
  }

  get f() {
    return this.form.controls;
  }

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.api.list().subscribe((services) => (this.services = services));
  }

  openCreate(): void {
    this.editing = null;
    this.submitted = false;
    this.form.reset({ speedMbps: 100, price: 0, active: true });
    this.showForm = true;
  }

  openEdit(service: ServicePlan): void {
    this.editing = service;
    this.submitted = false;
    this.form.reset({
      serviceName: service.serviceName,
      serviceDescription: service.serviceDescription,
      speedMbps: service.speedMbps,
      price: service.price,
      active: service.active,
      devices: service.devices.join(', '),
    });
    this.showForm = true;
  }

  save(): void {
    this.submitted = true;

    if (this.form.invalid) {
      return;
    }

    const value = this.form.getRawValue();
    const payload = {
      ...value,
      // Los equipos se escriben separados por coma
      devices: value.devices
        .split(',')
        .map((device) => device.trim())
        .filter((device) => device.length > 0),
    };

    const request = this.editing ? this.api.update(this.editing.serviceId, payload) : this.api.create(payload);

    request.subscribe(() => {
      this.notifications.success(this.editing ? 'Servicio actualizado' : 'Servicio creado');
      this.showForm = false;
      this.load();
    });
  }

  remove(service: ServicePlan): void {
    if (!confirm(`¿Eliminar el servicio ${service.serviceName}?`)) {
      return;
    }

    this.api.delete(service.serviceId).subscribe(() => {
      this.notifications.success('Servicio eliminado');
      this.load();
    });
  }
}
