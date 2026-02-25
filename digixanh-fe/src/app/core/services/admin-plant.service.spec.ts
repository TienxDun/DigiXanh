import { TestBed } from '@angular/core/testing';

import { AdminPlantService } from './admin-plant.service';

describe('AdminPlantService', () => {
  let service: AdminPlantService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(AdminPlantService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
