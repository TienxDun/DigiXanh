import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute } from '@angular/router';
import { of } from 'rxjs';
import { HttpClientTestingModule } from '@angular/common/http/testing';

import { AddPlantComponent } from './add-plant.component';

describe('AddPlantComponent', () => {
    let component: AddPlantComponent;
    let fixture: ComponentFixture<AddPlantComponent>;

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [AddPlantComponent, HttpClientTestingModule],
            providers: [
                {
                    provide: ActivatedRoute,
                    useValue: {
                        params: of({}),
                        queryParams: of({}),
                        snapshot: {
                            paramMap: {
                                get: () => null
                            },
                            queryParamMap: {
                                get: () => null
                            }
                        }
                    }
                }
            ]
        }).compileComponents();

        fixture = TestBed.createComponent(AddPlantComponent);
        component = fixture.componentInstance;
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });
});
