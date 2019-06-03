import { Component, OnInit } from '@angular/core';
import { UsersService } from '../../core/users.service';

@Component({
  selector: 'app-applicant-view',
  templateUrl: './applicant-view.component.html',
  styleUrls: ['./applicant-view.component.css']
})
export class ApplicantViewComponent implements OnInit {

  public values: string[];

  constructor(private usersService: UsersService) { }

  ngOnInit() {
    this.usersService.getApplicants().subscribe(result => {
      this.values = result;
    });
  }

}
