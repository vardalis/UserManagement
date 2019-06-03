import { Component, OnInit, OnDestroy } from '@angular/core';
import { Validators, FormBuilder } from '@angular/forms';
import { Router, ParamMap, ActivatedRoute } from '@angular/router';
import { UsersService } from '../../core/users.service';
import { User } from '../../shared/user.model';
import { Role } from '../../shared/role.model';
import { Observable, Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { DialogService } from 'src/app/core/dialog.service';
import { CanComponentDeactivate } from 'src/app/core/can-deactivate.guard';

@Component({
  selector: 'app-user-edit',
  templateUrl: './user-edit.component.html',
  styleUrls: ['./user-edit.component.css']
})
export class UserEditComponent implements OnInit, OnDestroy, CanComponentDeactivate {
  roles$: Observable<Role[]>;
  user$: Observable<User>;

  roles: Role[];
  user: User;

  submitAttempted = false;
  errors: string[] = [];
  originalUser: User;

  // Use it for automatic unsubscribing when destroying the component
  private onDestroy$: Subject<void> = new Subject<void>();

  userForm = this.fb.group({
    firstName: ['', Validators.required],
    lastName: ['', Validators.required],
    email: ['', [Validators.required, Validators.email]],
    role: ['', Validators.required],
    approved: ['', Validators.required]
  });

  get firstName() { return this.userForm.get('firstName'); }
  get lastName() { return this.userForm.get('lastName'); }
  get email() { return this.userForm.get('email'); }
  get role() { return this.userForm.get('role'); }
  get approved() { return this.userForm.get('approved'); }

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private usersService: UsersService,
    private dialogService: DialogService
  ) { }

  ngOnInit() {

    // Prefetch data
    this.route.data.subscribe((data: { resolvedValues: { user: User, roles: Role[] } }) => {
      this.user = data.resolvedValues.user;
      this.userForm.patchValue(this.user);

      this.roles = data.resolvedValues.roles;
    });
  }

  onSubmit() {
    this.submitAttempted = true;

    if (this.userForm.valid) {
      let user = new User();
      user.rowVersion = this.user.rowVersion;
      user.id = this.user.id;
      Object.assign(user, this.userForm.value);

      this.usersService.updateUser(user)
        .pipe(takeUntil(this.onDestroy$))
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

  ngOnDestroy(): void {
    this.onDestroy$.next();
    this.onDestroy$.complete();
  }
}
