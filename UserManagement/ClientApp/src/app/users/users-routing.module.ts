import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { AdminViewComponent } from './admin-view/admin-view.component';
import { ApplicantViewComponent } from './applicant-view/applicant-view.component';
import { AuthGuard } from '../core/auth.guard';
import { UserListComponent } from './user-list/user-list.component';
import { UserListResolverService, UserEditResolverService } from './users-resolvers.service';
import { UserCreateComponent } from './user-create/user-create.component';
import { UserDeleteComponent } from './user-delete/user-delete.component';
import { UserEditComponent } from './user-edit/user-edit.component';
import { PasswordChangeComponent } from './password-change/password-change.component';
import { CanDeactivateGuard } from '../core/can-deactivate.guard';

const routes: Routes = [
  {
    path: '',
    // canActivate: [AuthGuard],
    // component: UserManagementComponent,
    // data: { allowedRoles: ['admin', 'applicant']},
    children: [
      {
        path: 'admin-view',
        component: AdminViewComponent,
        canActivate: [AuthGuard],
        data: { allowedRoles: ['admin'] }
      },
      {
        path: 'applicant-view',
        component: ApplicantViewComponent,
        canActivate: [AuthGuard],
        data: { allowedRoles: ['applicant'] }
      },
      {
        path: '',
        runGuardsAndResolvers: 'paramsOrQueryParamsChange', // So that the resolver runs when the page changes
        component: UserListComponent,
        canActivate: [AuthGuard],
        data: { allowedRoles: ['admin'] },
        resolve: { userList: UserListResolverService }
      },
      {
        path: 'user-create',
        component: UserCreateComponent,
        canActivate: [AuthGuard],
        canDeactivate: [CanDeactivateGuard],
        data: { allowedRoles: ['admin'] }
      },
      {
        path: 'user-delete/:id',
        component: UserDeleteComponent,
        canActivate: [AuthGuard],
        data: { allowedRoles: ['admin'] }
      },
      {
        path: 'user-edit/:id',
        component: UserEditComponent,
        canDeactivate: [CanDeactivateGuard],
        canActivate: [AuthGuard],
        data: { allowedRoles: ['admin'] },
        resolve: { resolvedValues: UserEditResolverService }
      },
      {
        path: 'password-change/:id',
        component: PasswordChangeComponent,
        canActivate: [AuthGuard],
        data: { allowedRoles: ['admin'] }
      }
    ]
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class UsersRoutingModule { }
