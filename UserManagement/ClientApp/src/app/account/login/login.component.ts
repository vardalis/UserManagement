import { Component, OnInit } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { AccountService } from '../../core/account.service';
import { Router } from '@angular/router';
import { LoginRequest } from '../../shared/login-request.model';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent implements OnInit {
  status = this.accountService.isAuthenticated ? "Authenticated!" : "Unauthenticated!";
  submitAttempted = false;
  message = "";

  loginForm = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', Validators.required],
  });

  constructor(private fb: FormBuilder,
    private accountService: AccountService,
    private router: Router) { }

  ngOnInit() {
    this.loginForm.setValue({ email: '', password: '' });
  }

  onSubmit() {
    this.submitAttempted = true;

    if (this.loginForm.valid) {
      let loginVM: LoginRequest = {
        email: this.email.value,
        password: this.password.value,
      };

      this.accountService.login(loginVM)
        .subscribe(response => {
          this.status = this.accountService.isAuthenticated ? "Authenticated!" : "Unauthenticated!";
          this.message = response.message;

          if (this.accountService.isAuthenticated) {
            let redirect = this.accountService.redirectUrl ?
              this.router.parseUrl(this.accountService.redirectUrl) : '/';

            this.accountService.redirectUrl = null;
            this.router.navigateByUrl(redirect);
          }
        },
          error => { this.message = error });
    }
  }

  get email() { return this.loginForm.get('email'); }
  get password() { return this.loginForm.get('password'); }
}
