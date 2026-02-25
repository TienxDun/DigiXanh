import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { PlantDto } from '../models/plant.model';

@Injectable({
  providedIn: 'root'
})
export class PublicPlantService {
  private apiUrl = `${environment.apiUrl}/plants`;

  constructor(private http: HttpClient) { }

  getPlants(): Observable<PlantDto[]> {
    return this.http.get<PlantDto[]>(this.apiUrl);
  }
}
