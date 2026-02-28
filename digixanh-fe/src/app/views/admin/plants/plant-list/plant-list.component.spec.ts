import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, Router } from '@angular/router';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { of } from 'rxjs';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { vi } from 'vitest';

import { PlantListComponent } from './plant-list.component';
import { AdminPlantService } from '../../../../core/services/admin-plant.service';

describe('PlantListComponent', () => {
  let component: PlantListComponent;
  let fixture: ComponentFixture<PlantListComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PlantListComponent, HttpClientTestingModule],
      providers: [
        provideNoopAnimations(),
        {
          provide: ActivatedRoute,
          useValue: {
            params: of({}),
            queryParams: of({}),
            queryParamMap: of({
              get: () => null,
              keys: []
            }),
            snapshot: {
              paramMap: {
                get: () => null
              },
              queryParamMap: {
                get: () => null,
                keys: []
              }
            }
          }
        },
        {
          provide: Router,
          useValue: {
            navigate: vi.fn()
          }
        },
        {
          provide: AdminPlantService,
          useValue: {
            getPlants: vi.fn().mockReturnValue(of({
              items: [],
              totalCount: 0,
              page: 1,
              pageSize: 10,
              totalPages: 1
            }))
          }
        }
      ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PlantListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
