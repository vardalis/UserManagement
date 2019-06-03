import { Component, OnInit } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { samePasswordValidator } from 'src/app/shared/same-password.validator';
import { AccountService } from 'src/app/core/account.service';
import { Router } from '@angular/router';
import { Register } from '../../shared/register.model';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent implements OnInit {
  submitAttempted = false;
  formSubmitted = false;
  errors: string[] = [];

  registerForm = this.fb.group({
    firstName: ['', Validators.required],
    lastName: ['', Validators.required],
    email: ['', [Validators.required, Validators.email] ],
    password: ['', [Validators.required, Validators.minLength(6)]],
    confirmPassword: ['', Validators.required],
    recaptcha: [null, Validators.required]
  }, { validator: samePasswordValidator } );

  get firstName() { return this.registerForm.get('firstName'); }
  get lastName() { return this.registerForm.get('lastName'); }
  get email() { return this.registerForm.get('email'); }
  get password() { return this.registerForm.get('password'); }
  get confirmPassword() { return this.registerForm.get('confirmPassword'); }
  get recaptcha() { return this.registerForm.get('recaptcha'); }

  constructor(private fb: FormBuilder,
    private router: Router,
    private accountService: AccountService) {
  }

  ngOnInit() {
  }

  onSubmit() {
    this.submitAttempted = true;

    if (this.registerForm.valid) {
      let register: Register = {
        firstName: this.firstName.value,
        lastName: this.lastName.value,
        email: this.email.value,
        password: this.password.value,
        confirmPassword: this.confirmPassword.value,
        recaptcha: this.recaptcha.value
      };

      this.accountService.register(register)
        .subscribe(
        user => this.router.navigate(['/account/email-sent']),
        error => {
          for (var fieldName in error) {
            if (error.hasOwnProperty(fieldName)) {
              if (this.registerForm.controls[fieldName.toLowerCase()]) {
                // integrate into angular's validation if we have field validation
                this.registerForm.controls[fieldName.toLowerCase()].setErrors({ inUse: true });
              } else {
                // if we have cross field validation then show the validation error at the top of the screen
                this.errors.push(error[fieldName]);
              }
            }
          }

          this.formSubmitted = false;
        });

      this.formSubmitted = true;
    }
  }

  resolved(captchaResponse: string) {
    console.log(`Resolved captcha with response ${captchaResponse}:`);
  }
}
