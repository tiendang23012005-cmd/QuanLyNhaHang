import { ComponentFixture, TestBed } from '@angular/core/testing';

import { FoodManagement } from './food-management';

describe('FoodManagement', () => {
  let component: FoodManagement;
  let fixture: ComponentFixture<FoodManagement>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [FoodManagement]
    })
    .compileComponents();

    fixture = TestBed.createComponent(FoodManagement);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
