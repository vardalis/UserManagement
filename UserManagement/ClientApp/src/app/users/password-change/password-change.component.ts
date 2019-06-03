import { Component, OnInit } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { UsersService } from '../../core/users.service';
import { PasswordChange } from '../../shared/password-change.model';
import { samePasswordValidator } from 'src/app/shared/same-password.validator';

@Component({
  selector: 'app-password-change',
  templateUrl: './password-change.component.html',
  styleUrls: ['./password-change.component.css']
})
export class PasswordChangeComponent implements OnInit {
  userId: string;
  submitAttempted = false;

  passwordForm = this.fb.group({
    password: ['', [Validators.required, Validators.minLength(6)]],
    confirmPassword: ['', Validators.required]
  }, { validator: samePasswordValidator });

  get password() { return this.passwordForm.get('password'); }
  get confirmPassword() { return this.passwordForm.get('confirmPassword'); }

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private usersService: UsersService
  ) { }

  ngOnInit() {
    this.route.paramMap.subscribe(params => this.userId = params.get('id'));
  }

  onCancel() {
    this.router.navigate(['/users'])
  }

  onSubmit() {
    this.submitAttempted = true;
 
    if (this.passwordForm.valid) {
      let passwordChangeVm: PasswordChange = {
        password: this.password.value,
        confirmPassword: this.confirmPassword.value,
      };

      this.usersService.changePassword(this.userId, passwordChangeVm)
        .subscribe(user => this.router.navigate(['/users']));

    }
  }
}
