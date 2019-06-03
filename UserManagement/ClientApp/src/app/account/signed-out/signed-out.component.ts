import { Component, OnInit } from '@angular/core';
import { AccountService } from '../../core/account.service';

@Component({
  selector: 'app-signed-out',
  templateUrl: './signed-out.component.html',
  styleUrls: ['./signed-out.component.css']
})
export class SignedOutComponent implements OnInit {

  constructor(private accountService: AccountService)
  {
    this.accountService.signOut();
  }

  ngOnInit() {    
  }
}
