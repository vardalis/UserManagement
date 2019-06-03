import { Component, OnInit } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { ActivatedRoute, Router, Params } from '@angular/router';
import { User } from '../../shared/user.model';
import { PageUsers } from '../../shared/page-users.model';
import { map } from 'rxjs/operators';
import { TranslateService } from '@ngx-translate/core';

@Component({
  selector: 'app-user-list',
  templateUrl: './user-list.component.html',
  styleUrls: ['./user-list.component.css']
})
export class UserListComponent implements OnInit {

  userList: User[];
  userPage: User[];
  totalUsers: number;
  page: number = 1;
  sortOrder: string = 'Fname';
  searchString: string = '';
  pageSize: number = 10;

  searchForm = this.fb.group({
    searchString: ['']
  });

  constructor(
    private router: Router,
    private route: ActivatedRoute,
    private fb: FormBuilder,
    public translate: TranslateService
  ) { }

  ngOnInit() {
    this.route.queryParamMap
      .pipe(
        map(params => {
          this.page = + params.get('page') || 1;
          this.pageSize = + params.get('pageSize') || 10;
          this.sortOrder = params.get('sortOrder') || 'Fname';
          this.searchString = params.get('searchString') || '';
        }))
      .subscribe();

    // Prefetch data
    this.route.data.subscribe((data: { userList: PageUsers }) => {
      this.userPage = data.userList.users;
      this.totalUsers = data.userList.totalUsers;
    });
  }

  onSearch() {
    let params: Params = {
      page: '1',
      searchString: this.searchForm.get('searchString').value
    }

    // Update route with the page number and allow page bookmarking for user convenience 
    this.router.navigate(
      [],
      {
        // relativeTo: this.route,
        queryParams: params,
        queryParamsHandling: 'merge'
      }
    );
  }

  onSearchReset() {
    this.searchForm.get('searchString').setValue('');

    let params: Params = {
      page: '1',
      searchString: ''
    }

    // Update route with the page number and allow page bookmarking for user convenience 
    this.router.navigate(
      [],
      {
        // relativeTo: this.route,
        queryParams: params,
        queryParamsHandling: 'merge'
      }
    );
  }

  onHeaderClick(header: string) {
    let newSortOrder = this.sortOrder;

    switch (header) {
      case 'Fname': {
        newSortOrder = this.sortOrder == 'Fname' ? 'Fname_desc' : 'Fname';
        break;
      }
      case 'Lname': {
        newSortOrder = this.sortOrder == 'Lname' ? 'Lname_desc' : 'Lname';
        break;
      }
      case 'Email': {
        newSortOrder = this.sortOrder == 'Email' ? 'Email_desc' : 'Email';
        break;
      }
      case 'Approved': {
        newSortOrder = this.sortOrder == 'Approved' ? 'Approved_desc' : 'Approved';
        break;
      }
    }

    let params: Params = {
      page: '1',
      sortOrder: newSortOrder
    }

    // Update route with the page number and allow page bookmarking for user convenience 
    this.router.navigate(
      [],
      {
        // relativeTo: this.route,
        queryParams: params,
        queryParamsHandling: 'merge'
      }
    );

  }

  onChangePage(page: number) {

    let params: Params = {
      page: page.toString()
    }

    // Update route with the page number and allow page bookmarking for user convenience 
    this.router.navigate(
      [],
      {
        // relativeTo: this.route,
        queryParams: params,
        queryParamsHandling: 'merge'
      }
    );
  }
}
