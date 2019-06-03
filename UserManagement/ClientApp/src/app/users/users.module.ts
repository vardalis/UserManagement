import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';

import { AdminViewComponent } from './admin-view/admin-view.component';
import { ApplicantViewComponent } from './applicant-view/applicant-view.component';

import { UsersRoutingModule } from './users-routing.module';
import { UserListComponent } from './user-list/user-list.component';

import { SharedModule } from '../shared/shared.module';
import { UserCreateComponent } from './user-create/user-create.component';
import { UserDeleteComponent } from './user-delete/user-delete.component';
import { UserEditComponent } from './user-edit/user-edit.component';
import { PasswordChangeComponent } from './password-change/password-change.component';
import { TranslateModule } from '@ngx-translate/core';

@NgModule({
  declarations: [
    AdminViewComponent,
    ApplicantViewComponent,
    UserListComponent,
    UserCreateComponent,
    UserDeleteComponent,
    UserEditComponent,
    PasswordChangeComponent
  ],
  imports: [
    CommonModule,
    ReactiveFormsModule,
    TranslateModule.forChild(),
    UsersRoutingModule,
    SharedModule
  ]
})
export class UsersModule { }
