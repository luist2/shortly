import { ComponentFixture, TestBed } from '@angular/core/testing';

import { UrlStatsComponent } from './url-stats.component';

describe('UrlStatsComponent', () => {
  let component: UrlStatsComponent;
  let fixture: ComponentFixture<UrlStatsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ UrlStatsComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(UrlStatsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
