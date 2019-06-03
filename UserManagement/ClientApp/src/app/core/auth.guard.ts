import { Injectable } from '@angular/core';
import {
  CanActivate, CanLoad, Data, ActivatedRouteSnapshot,
  RouterStateSnapshot, Route, Router
} from '@angular/router';
import { Observable } from 'rxjs';
import { AccountService } from './account.service';

@Injectable({
  providedIn: 'root'
})
export class AuthGuard implements CanActivate, CanLoad {
  constructor(private accountService: AccountService, private router: Router) { }

  canActivate(
    next: ActivatedRouteSnapshot,
    state: RouterStateSnapshot): Observable<boolean> | Promise<boolean> | boolean {

    return this.checkLogin(state.url, next);
  }

  canLoad(route: Route): boolean {
    return this.accountService.isAuthenticated &&
      route.data.allowedRoles.includes(this.accountService.role)
  }

  checkLogin(url: string, next: ActivatedRouteSnapshot): boolean {
    if (this.accountService.isAuthenticated &&
      next.data.allowedRoles.includes(this.accountService.role))
      return true;

    this.accountService.redirectUrl = url;

    this.router.navigate(['/account/login']);
    return false;
  }
}
