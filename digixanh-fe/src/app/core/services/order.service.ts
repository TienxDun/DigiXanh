import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { 
  CreateOrderRequest, 
  CreateOrderResponse, 
  VNPayReturnResponse 
} from '../models/order.model';

@Injectable({
  providedIn: 'root'
})
export class OrderService {
  private readonly apiUrl = `${environment.apiUrl}/orders`;
  private readonly paymentApiUrl = `${environment.apiUrl}/payment`;

  constructor(private readonly http: HttpClient) {}

  createOrder(request: CreateOrderRequest): Observable<CreateOrderResponse> {
    return this.http.post<CreateOrderResponse>(this.apiUrl, request);
  }

  processVNPayReturn(vnpayData: { [key: string]: string }): Observable<VNPayReturnResponse> {
    // Chuyển query params thành HTTP params
    let params = new HttpParams().set('format', 'json');
    Object.keys(vnpayData).forEach(key => {
      params = params.set(key, vnpayData[key]);
    });
    
    return this.http.get<VNPayReturnResponse>(`${this.paymentApiUrl}/vnpay-return`, { params });
  }
}
