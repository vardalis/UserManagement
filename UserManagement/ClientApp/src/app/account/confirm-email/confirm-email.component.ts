import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, ParamMap } from '@angular/router';
import { Observable, Subject } from 'rxjs';
import { map, takeUntil, switchMap } from 'rxjs/operators';
import { AccountService } from '../../core/account.service';

@Component({
  selector: 'app-confirm-email',
  templateUrl: './confirm-email.component.html',
  styleUrls: ['./confirm-email.component.css']
})
export class ConfirmEmailComponent implements OnInit {
  // Use it for automatic unsubscribing when destroying the component
  private onDestroy$: Subject<void> = new Subject<void>();

  requestReturned = false;
  userId: string;
  code: string;

  constructor(
    private route: ActivatedRoute,
    private accountService: AccountService) { }

  ngOnInit() {
    this.route.queryParamMap.pipe(
      takeUntil(this.onDestroy$),
      switchMap((queryParams: ParamMap) => {
        this.userId = queryParams.get('userId');
        this.code = queryParams.get('code');

        return this.accountService.confirmEmail(queryParams.get ('userId'),
          queryParams.get('code'));
      })
    )
    .subscribe(() => this.requestReturned = true);
  }
}
