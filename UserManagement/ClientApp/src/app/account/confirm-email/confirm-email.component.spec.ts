import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ConfirmEmailComponent } from './confirm-email.component';
import { ActivatedRouteStub } from '../../../testing/activated-route-stub';
import { ActivatedRoute } from '@angular/router';
import { AccountService } from 'src/app/core/account.service';
import { Observable, of } from 'rxjs';

describe('ConfirmEmailComponent', () => {
  let component: ConfirmEmailComponent;
  let fixture: ComponentFixture<ConfirmEmailComponent>;

  let accountService: Partial<AccountService>;
  let activatedRoute: ActivatedRouteStub ;

  beforeEach(async(() => {
    accountService = {
      confirmEmail(userId: string, code: string) {
        return of(true);
      }
    };

    activatedRoute = new ActivatedRouteStub({ userId: "testId", code: "testCode" });

    TestBed.configureTestingModule({
      declarations: [ConfirmEmailComponent],
      providers: [
        { provide: ActivatedRoute, useValue: activatedRoute },
        { provide: AccountService, useValue: accountService }
      ]
    })
      .compileComponents();
  }));

  beforeEach(() => {
    activatedRoute = TestBed.get(ActivatedRoute);
    accountService = TestBed.get(AccountService);

    fixture = TestBed.createComponent(ConfirmEmailComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should set requestReturn to true', () => {
    expect(component.requestReturned).toBeTruthy();
  });

});
