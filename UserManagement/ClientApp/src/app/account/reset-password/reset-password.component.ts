import { Component, OnInit } from '@angular/core';
import { Validators, FormBuilder } from '@angular/forms';
import { samePasswordValidator } from 'src/app/shared/same-password.validator';
import { AccountService } from '../../core/account.service';
import { Router, Route, ParamMap, ActivatedRoute } from '@angular/router';
import { ResetPassword } from '../../shared/reset-password.model';
import { Subject } from 'rxjs';
import { takeUntil, switchMap } from 'rxjs/operators';

@Component({
  selector: 'app-reset-password',
  templateUrl: './reset-password.component.html',
  styleUrls: ['./reset-password.component.css']
})
export class ResetPasswordComponent implements OnInit {
  // Use it for automatic unsubscribing when destroying the component
  private onDestroy$: Subject<void> = new Subject<void>();

  submitAttempted = false;
  submitSucceeded = false;

  resetPasswordForm = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]],
    confirmPassword: ['', Validators.required],
  }, { validator: samePasswordValidator });

  get email() { return this.resetPasswordForm.get('email'); }
  get password() { return this.resetPasswordForm.get('password'); }
  get confirmPassword() { return this.resetPasswordForm.get('confirmPassword'); }

  constructor(private fb: FormBuilder,
    private route: ActivatedRoute,
    private accountService: AccountService) { }

  ngOnInit() {

  }

  onSubmit() {
    this.submitAttempted = true;
    if (this.resetPasswordForm.valid) {
      this.route.queryParamMap.pipe(
        takeUntil(this.onDestroy$),
        switchMap((queryParams: ParamMap) => {

          let resetPassword: ResetPassword = {
            email: this.email.value,
            password: this.password.value,
            confirmPassword: this.confirmPassword.value,
            code: queryParams.get('code')
          };

          return this.accountService.resetPassword(resetPassword);
        }))
        .pipe(takeUntil(this.onDestroy$))
        .subscribe(() => this.submitSucceeded = true);
    }
  }
}
