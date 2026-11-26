import { Component } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { NotificationService } from '../../core/services/ui.service';

@Component({
  selector: 'app-login',
  standalone: false,
  templateUrl: './login.component.html',
  styleUrl: './auth.scss',
})
export class LoginComponent {
  submitted = false;

  form;

  constructor(
    private fb: FormBuilder,
    private auth: AuthService,
    private router: Router,
    private notifications: NotificationService
  ) {
    this.form = this.fb.nonNullable.group({
      userName: ['', [Validators.required, Validators.minLength(8), Validators.maxLength(20), Validators.pattern(/^[A-Za-z0-9]+$/)]],
      password: ['', [Validators.required, Validators.minLength(8), Validators.maxLength(30)]],
    });
  }

  get f() {
    return this.form.controls;
  }

  login(): void {
    this.submitted = true;

    if (this.form.invalid) {
      return;
    }

    const { userName, password } = this.form.getRawValue();

    this.auth.login(userName.trim(), password).subscribe((session) => {
      this.notifications.success(`Bienvenido, ${session.fullName}`);
      this.router.navigate(['/home']);
    });
  }
}
