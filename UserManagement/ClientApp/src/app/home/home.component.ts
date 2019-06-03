import { Component, OnInit } from '@angular/core';
import { NotifierService } from 'angular-notifier';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css']
})
export class HomeComponent implements OnInit {

  constructor(private notifier: NotifierService) { }

  ngOnInit() {
    this.notifier.notify('success', 'You are awesome! I mean it!');
  }
}
