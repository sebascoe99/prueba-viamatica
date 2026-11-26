import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { Client } from '../../core/models';
import { ClientApi } from '../../core/services/api.service';
import { NotificationService } from '../../core/services/ui.service';

@Component({
  selector: 'app-client-form',
  standalone: false,
  templateUrl: './client-form.component.html',
})
export class ClientFormComponent implements OnInit {
  @Input() client: Client | null = null;
  @Input() identification = '';
  @Output() saved = new EventEmitter<Client>();
  @Output() cancelled = new EventEmitter<void>();

  submitted = false;
  form;

  constructor(private fb: FormBuilder, private api: ClientApi, private notifications: NotificationService) {
    this.form = this.fb.nonNullable.group({
      name: ['', [Validators.required, Validators.maxLength(50)]],
      lastName: ['', [Validators.required, Validators.maxLength(50)]],
      identification: ['', [Validators.required, Validators.pattern(/^[0-9]{10,13}$/)]],
      email: ['', [Validators.required, Validators.email, Validators.maxLength(120)]],
      phoneNumber: ['', [Validators.required, Validators.pattern(/^09[0-9]{8,11}$/)]],
      address: ['', [Validators.required, Validators.minLength(20), Validators.maxLength(100)]],
      referenceAddress: ['', [Validators.required, Validators.minLength(20), Validators.maxLength(100)]],
    });
  }

  get f() {
    return this.form.controls;
  }

  ngOnInit(): void {
    if (this.client) {
      this.form.patchValue(this.client);
    } else if (this.identification) {
      this.form.patchValue({ identification: this.identification });
    }
  }

  save(): void {
    this.submitted = true;

    if (this.form.invalid) {
      return;
    }

    const value = this.form.getRawValue();
    const request = this.client ? this.api.update(this.client.clientId, value) : this.api.create(value);

    request.subscribe((client) => {
      this.notifications.success(this.client ? 'Cliente actualizado' : 'Cliente registrado');
      this.saved.emit(client);
    });
  }
}
