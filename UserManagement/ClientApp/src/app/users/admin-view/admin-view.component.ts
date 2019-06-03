import { Component, OnInit } from '@angular/core';
import { UsersService } from '../../core/users.service';

@Component({
  selector: 'app-admin-view',
  templateUrl: './admin-view.component.html',
  styleUrls: ['./admin-view.component.css']
})
export class AdminViewComponent implements OnInit {

  public values: string[];

  constructor(private usersService: UsersService) { }

  ngOnInit() {
    this.usersService.getAdmins().subscribe(result => {
      this.values = result;
    });
  }

}
