import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { SignedOutComponent } from './signed-out.component';

describe('SignedOutComponent', () => {
  let component: SignedOutComponent;
  let fixture: ComponentFixture<SignedOutComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ SignedOutComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(SignedOutComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
