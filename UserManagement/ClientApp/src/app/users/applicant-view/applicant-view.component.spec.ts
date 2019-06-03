import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ApplicantViewComponent } from './applicant-view.component';

describe('ApplicantViewComponent', () => {
  let component: ApplicantViewComponent;
  let fixture: ComponentFixture<ApplicantViewComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ApplicantViewComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ApplicantViewComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
