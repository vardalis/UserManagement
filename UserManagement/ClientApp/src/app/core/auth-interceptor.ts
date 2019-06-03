import { Injectable } from '@angular/core';
import {
  HttpEvent, HttpInterceptor, HttpHandler, HttpRequest
} from '@angular/common/http';

import { Observable } from 'rxjs';
import { AccountService } from './account.service';

@Injectable({
  providedIn: 'root',
})
export class AuthInterceptor implements HttpInterceptor {

  constructor(private accountService: AccountService) { }

  intercept(origRequest: HttpRequest<any>, next: HttpHandler):
    Observable<HttpEvent<any>> {

    if (this.accountService.isAuthenticated) {
      const clonedRequest = origRequest.clone({
        headers: origRequest.headers.set("Authorization", "Bearer " + this.accountService.token)
      });

      return next.handle(clonedRequest);
    }
    else {
      return next.handle(origRequest);
    }
  }
}
