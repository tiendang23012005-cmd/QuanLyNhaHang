import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CreateFood } from './create-food';

describe('CreateFood', () => {
  let component: CreateFood;
  let fixture: ComponentFixture<CreateFood>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CreateFood]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CreateFood);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
