import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpErrorResponse, HttpParams } from '@angular/common/http';
import { catchError } from 'rxjs/operators';
import { of, Observable, throwError } from 'rxjs';
import { User } from '../shared/user.model';
import { PageUsers } from '../shared/page-users.model';
import { Role } from '../shared/role.model';
import { PasswordChange } from '../shared/password-change.model';

@Injectable({
  providedIn: 'root'
})
export class UsersService {

  constructor(private http: HttpClient) { }

  private handleError(error: HttpErrorResponse) {

    if (error.error instanceof ErrorEvent) {
      // A client-side or network error occurred. Handle it accordingly.
      console.error('An error occurred:', error.error.message);
      return throwError('Something bad happened; please try again later.');
    } else if (error.status == 400) {
      // Model validation error, return validation messages.
      return throwError(error.error);
    }

    // The backend returned an unsuccessful response code.
    // The response body may contain clues as to what went wrong.
    console.error(
      `Backend returned code ${error.status}, ` +
      `body was: ${error.error}`);

    // return an observable with a user-facing error message
    return throwError(
      'Something bad happened; please try again later.');
  };

  getAdmins() {

    const httpOptions = {
      headers: new HttpHeaders({
        'Content-Type': 'application/json'
      })
    };

    const url = window['baseUrl'] + "/api/users/admins";
    // const url = "https://localhost:44374/api/UserManagement/Admins";

    return this.http.get<string[]>(url, httpOptions)
      .pipe(catchError(this.handleError));
  }

  getApplicants() {

    const httpOptions = {
      headers: new HttpHeaders({
        'Content-Type': 'application/json'
      })
    };

    const url = window['baseUrl'] + "/api/users/applicants";
    // const url = "https://localhost:44374/api/UserManagement/Applicants";

    return this.http.get<string[]>(url, httpOptions)
      .pipe(catchError(this.handleError));
  }


  getAllUsers() {

    const httpOptions = {
      headers: new HttpHeaders({
        'Content-Type': 'application/json'
      })
    };

    const url = window['baseUrl'] + "/api/users";
    // const url = "https://localhost:44374/api/UserManagement/Users";

    return this.http.get<PageUsers>(url, httpOptions)
      .pipe(catchError(this.handleError));
  }

  getPageUsers(pageNumber: number, pageSize: number, sortOrder: string, searchString: string): Observable<PageUsers> {
    const offset = (pageNumber - 1) * pageSize;
    const limit = pageSize;


    const httpOptions = {
      headers: new HttpHeaders({
        'Content-Type': 'application/json'
      }),
      params: new HttpParams()
        .set('offset', offset.toString())
        .set('limit', limit.toString())
        .set('sortOrder', sortOrder)
        .set('searchString', searchString)
    };

    const url = window['baseUrl'] + "/api/users";
    // const url = "https://localhost:44374/api/UserManagement/Users";

    return this.http.get<PageUsers>(url, httpOptions)
      .pipe(catchError(this.handleError));
  }

  getRoles(): Observable<Role[]> {

    const httpOptions = {
      headers: new HttpHeaders({
        'Content-Type': 'application/json'
      })};

    const url = window['baseUrl'] + "/api/users/roles";

    return this.http.get<Role[]>(url, httpOptions)
      .pipe(catchError(this.handleError));
  }

  createUser(userVm: User) {

    const httpOptions = {
      headers: new HttpHeaders({
        'Content-Type': 'application/json'
      })
    };

    const url = window['baseUrl'] + "/api/users";

    return this.http.post<User>(url, userVm, httpOptions)
      .pipe(catchError(this.handleError));
  }

  deleteUser(id: string) {

    const httpOptions = {
      headers: new HttpHeaders({
        'Content-Type': 'application/json'
      })
    };

    const url = window['baseUrl'] + "/api/users/" + id;

    return this.http.delete<string>(url, httpOptions)
      .pipe(catchError(this.handleError));
  }

  getUser(id: string): Observable<User> {

    const httpOptions = {
      headers: new HttpHeaders({
        'Content-Type': 'application/json'
      })
    };

    const url = window['baseUrl'] + "/api/users/" + id;

    return this.http.get<User>(url, httpOptions)
      .pipe(catchError(this.handleError));
  }

  updateUser(userVm: User) {

    const httpOptions = {
      headers: new HttpHeaders({
        'Content-Type': 'application/json'
      })
    };

    const url = window['baseUrl'] + "/api/users/" + userVm.id;

    return this.http.put<User>(url, userVm, httpOptions)
      .pipe(catchError(this.handleError));
  }

  changePassword(userId: string, passwordChange: PasswordChange) {
    const httpOptions = {
      headers: new HttpHeaders({
        'Content-Type': 'application/json'
      })
    };

    const url = window['baseUrl'] + "/api/users/" + userId + "/change-password";

    return this.http.post<User>(url, passwordChange, httpOptions)
      .pipe(catchError(this.handleError));
  }
}
