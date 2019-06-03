import { Component, OnInit } from '@angular/core';
import { Validators, FormBuilder } from '@angular/forms';
import { AccountService } from '../../core/account.service';
import { NotifierService } from 'angular-notifier';

@Component({
  selector: 'app-forgot-password',
  templateUrl: './forgot-password.component.html',
  styleUrls: ['./forgot-password.component.css']
})
export class ForgotPasswordComponent implements OnInit {
  submitAttempted = false;
  submitSucceeded = false;

  emailForm = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
  });

  get email() { return this.emailForm.get('email'); }

  constructor(
    private fb: FormBuilder,
    private accountService: AccountService,
    private notifier: NotifierService
  ) { }

  ngOnInit() {
  }

  onSubmit() {
    this.submitAttempted = true;

    if (this.emailForm.valid) {
      this.accountService.forgotPassword(this.email.value)
        .subscribe(() => {
          this.submitSucceeded = true;
        },
        error => {
          /* Notify error */
          this.notifier.notify('error', 'Email not sent! Please try again later...');
        });
    }
  }
}
