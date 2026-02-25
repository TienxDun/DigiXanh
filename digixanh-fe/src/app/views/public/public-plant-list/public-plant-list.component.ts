import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { catchError, Observable, of } from 'rxjs';
import { PlantDto } from '../../../core/models/plant.model';
import { PublicPlantService } from '../../../core/services/public-plant.service';

@Component({
  selector: 'app-public-plant-list',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './public-plant-list.component.html',
  styleUrl: './public-plant-list.component.scss'
})
export class PublicPlantListComponent {
  plants$: Observable<PlantDto[]>;

  constructor(private publicPlantService: PublicPlantService) {
    this.plants$ = this.publicPlantService.getPlants().pipe(
      catchError(() => of([]))
    );
  }
}