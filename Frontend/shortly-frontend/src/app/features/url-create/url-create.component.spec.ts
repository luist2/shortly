import { ComponentFixture, TestBed } from '@angular/core/testing';

import { UrlCreateComponent } from './url-create.component';

describe('UrlCreateComponent', () => {
  let component: UrlCreateComponent;
  let fixture: ComponentFixture<UrlCreateComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ UrlCreateComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(UrlCreateComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
