import { Injectable } from '@angular/core';
import { UsersService } from '../core/users.service';
import { Resolve, RouterStateSnapshot, ActivatedRouteSnapshot, Router } from '@angular/router';
import { User } from '../shared/user.model';
import { PageUsers } from '../shared/page-users.model';
import { Observable, of, forkJoin } from 'rxjs';
import { take, map, catchError } from 'rxjs/operators';
import { Role } from '../shared/role.model';

@Injectable({
  providedIn: 'root'
})
export class UserListResolverService implements Resolve<PageUsers> {

  constructor(private usersService: UsersService) { }

  resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot):
    Observable<PageUsers> | Observable<never> {

    let page = +route
      .queryParamMap
      .get('page') || 1;

    let pageSize = +route
      .queryParamMap
      .get('pageSize') || 10;

    let sortOrder = route
      .queryParamMap
      .get('sortOrder') || 'Fname';

    let searchString = route
      .queryParamMap
      .get('searchString') || '';

    return this.usersService.getPageUsers(page, pageSize, sortOrder, searchString)
      .pipe(take(1));
  }
}

@Injectable({
  providedIn: 'root'
})
export class UserEditResolverService implements Resolve<{ user: User, roles: Role[] }> {

  constructor(
    private usersService: UsersService,
    private router: Router
  ) { }

  resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot):
    Observable<{ user: User, roles: Role[] }> {

    let userId = route.paramMap.get('id');

    //return this.userManagementService.getUser(userId).pipe(
    //  take(1),
    //  mergeMap(user => {
    //    if (user) {
    //      return of(user)
    //    } else {
    //      this.router.navigate(['/users'])
    //    }
    //  })
    //);

    return forkJoin([
        this.usersService.getUser(userId),
        this.usersService.getRoles()
      ])
      .pipe(
        catchError(error => this.router.navigate(['/users'])), // Navigate to users list at error
        take(1),
        map(results => ({
          user: results[0],
          roles: results[1]
        }))
      );
  }
}
