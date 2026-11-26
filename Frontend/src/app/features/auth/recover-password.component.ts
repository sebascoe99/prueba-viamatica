import { Component } from '@angular/core';
import { AbstractControl, FormBuilder, ValidationErrors, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { NotificationService } from '../../core/services/ui.service';

function passwordsMatch(group: AbstractControl): ValidationErrors | null {
  const password = group.get('newPassword')?.value;
  const confirm = group.get('confirmPassword')?.value;
  return password === confirm ? null : { mismatch: true };
}

@Component({
  selector: 'app-recover-password',
  standalone: false,
  templateUrl: './recover-password.component.html',
  styleUrl: './auth.scss',
})
export class RecoverPasswordComponent {
  submitted = false;
  form;

  constructor(
    private fb: FormBuilder,
    private auth: AuthService,
    private router: Router,
    private notifications: NotificationService
  ) {
    this.form = this.fb.nonNullable.group(
      {
        userName: ['', [Validators.required]],
        identification: ['', [Validators.required, Validators.pattern(/^[0-9]{10,13}$/)]],
        email: ['', [Validators.required, Validators.email]],
        newPassword: ['', [Validators.required, Validators.pattern(/^(?=.*[0-9])(?=.*[A-Z]).{8,30}$/)]],
        confirmPassword: ['', [Validators.required]],
      },
      { validators: passwordsMatch }
    );
  }

  get f() {
    return this.form.controls;
  }

  recover(): void {
    this.submitted = true;

    if (this.form.invalid) {
      return;
    }

    const { userName, identification, email, newPassword } = this.form.getRawValue();

    this.auth.recoverPassword({ userName: userName.trim(), identification, email: email.trim(), newPassword }).subscribe((response) => {
      this.notifications.success(response.message);
      this.router.navigate(['/auth/login']);
    });
  }
}
