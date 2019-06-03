import { Component, OnInit } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { User } from '../../shared/user.model';
import { Role } from '../../shared/role.model';
import { Router } from '@angular/router';
import { UsersService } from '../../core/users.service';
import { Observable } from 'rxjs';
import { samePasswordValidator } from 'src/app/shared/same-password.validator';
import { CanComponentDeactivate } from 'src/app/core/can-deactivate.guard';
import { DialogService } from 'src/app/core/dialog.service';

@Component({
  selector: 'app-user-create',
  templateUrl: './user-create.component.html',
  styleUrls: ['./user-create.component.css']
})
export class UserCreateComponent implements OnInit, CanComponentDeactivate {
  roles$: Observable<Role[]>;
  submitAttempted = false;
  errors: string[] = [];

  userForm = this.fb.group({
    firstName: ['', Validators.required],
    lastName: ['', Validators.required],
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]],
    confirmPassword: ['', Validators.required],
    role: ['', Validators.required],
    approved: ['', Validators.required]
  }, { validator: samePasswordValidator } );


  get firstName() { return this.userForm.get('firstName'); }
  get lastName() { return this.userForm.get('lastName'); }
  get email() { return this.userForm.get('email'); }
  get password() { return this.userForm.get('password'); }
  get confirmPassword() { return this.userForm.get('confirmPassword'); }
  get role() { return this.userForm.get('role'); }
  get approved() { return this.userForm.get('approved'); }

  constructor(
    private fb: FormBuilder,
    // private route: ActivatedRoute,
    private router: Router,
    private usersService: UsersService,
    private dialogService: DialogService
  ) {
    this.approved.setValue(false);
  }

  ngOnInit() {
    this.roles$ = this.usersService.getRoles();
  }

  onSubmit() {
    this.submitAttempted = true;

    if (this.userForm.valid) {
      let user = new User();
      Object.assign(user, this.userForm.value);
      user.role = this.role.value.name; // Otherwise an object will be assigned

      //let user: User = {
      //  rowVersion: '',
      //  id: null,
      //  firstName: this.firstName.value,
      //  lastName: this.lastName.value,
      //  email: this.email.value,
      //  password: this.password.value,
      //  confirmPassword: this.confirmPassword.value,
      //  role: this.role.value.name,
      //  approved: this.approved.value
      //};

      this.usersService.createUser(user)
        .subscribe(
          createdUser => {
            this.userForm.markAsPristine(); // So that navigating away is allowed by the guard after saving
            this.router.navigate(['/users']);
          },
          error => {
            for (var fieldName in error) {
              if (error.hasOwnProperty(fieldName)) {
                if (this.userForm.controls[fieldName.toLowerCase()]) {
                  // integrate into angular's validation if we have field validation
                  this.userForm.controls[fieldName.toLowerCase()].setErrors({ inUse: true });
                } else {
                  // if we have cross field validation then show the validation error at the top of the screen
                  this.errors.push(error[fieldName]);
                }
              }
            }
          }
      );
    }
  }

  onCancel() {
    this.router.navigate(['/users']);
  }

  canDeactivate(): Observable<boolean> | boolean {
    // Allow synchronous navigation (`true`) if no crisis or the crisis is unchanged
    if (!this.userForm.dirty) {
      return true;
    }
    // Otherwise ask the user with the dialog service and return its
    // observable which resolves to true or false when the user decides
    return this.dialogService.confirm('Discard changes?');
  }
}
