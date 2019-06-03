import { Component, OnInit } from '@angular/core';
import { Router, ActivatedRoute, ParamMap } from '@angular/router';
import { UsersService } from '../../core/users.service';

@Component({
  selector: 'app-user-delete',
  templateUrl: './user-delete.component.html',
  styleUrls: ['./user-delete.component.css']
})
export class UserDeleteComponent implements OnInit {
  userId: string;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private usersService: UsersService
  ) { }

  ngOnInit() {
    this.route.paramMap.subscribe(params => this.userId = params.get('id'));
  }

  onYes() {
    this.usersService.deleteUser(this.userId)
      .subscribe(user => this.router.navigate(['/users']));
  }

  onNo() {
    this.router.navigate(['/users'])
  }
}
