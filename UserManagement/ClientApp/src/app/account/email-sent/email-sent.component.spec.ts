import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { EmailSentComponent } from './email-sent.component';

describe('EmailSentComponent', () => {
  let component: EmailSentComponent;
  let fixture: ComponentFixture<EmailSentComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ EmailSentComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(EmailSentComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
