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

  constructor(private readonly http: HttpClient) {}

  createOrder(request: CreateOrderRequest): Observable<CreateOrderResponse> {
    // Set return URL cho VNPay
    const returnUrl = `${window.location.origin}/payment-return`;
    request.returnUrl = returnUrl;
    
    return this.http.post<CreateOrderResponse>(this.apiUrl, request);
  }

  processVNPayReturn(vnpayData: { [key: string]: string }): Observable<VNPayReturnResponse> {
    // Chuyển query params thành HTTP params
    let params = new HttpParams();
    Object.keys(vnpayData).forEach(key => {
      params = params.set(key, vnpayData[key]);
    });
    
    return this.http.get<VNPayReturnResponse>(`${this.apiUrl}/payment-return`, { params });
  }
}
