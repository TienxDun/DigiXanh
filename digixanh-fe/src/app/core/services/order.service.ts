import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { PagedResult } from '../models/pagination.model';
import { 
  CreateOrderRequest, 
  CreateOrderResponse, 
  VNPayReturnResponse,
  AdminOrderDto,
  AdminOrderDetailDto,
  UpdateOrderStatusRequest,
  OrderStatusOption,
  AdminOrderQueryParams
} from '../models/order.model';

@Injectable({
  providedIn: 'root'
})
export class OrderService {
  private readonly apiUrl = `${environment.apiUrl}/orders`;
  private readonly adminApiUrl = `${environment.apiUrl}/admin/orders`;
  private readonly paymentApiUrl = `${environment.apiUrl}/payment`;

  constructor(private readonly http: HttpClient) {}

  // ============== User Endpoints ==============
  
  createOrder(request: CreateOrderRequest): Observable<CreateOrderResponse> {
    return this.http.post<CreateOrderResponse>(this.apiUrl, request);
  }

  processVNPayReturn(vnpayData: { [key: string]: string }): Observable<VNPayReturnResponse> {
    let params = new HttpParams().set('format', 'json');
    Object.keys(vnpayData).forEach(key => {
      params = params.set(key, vnpayData[key]);
    });
    
    return this.http.get<VNPayReturnResponse>(`${this.paymentApiUrl}/vnpay-return`, { params });
  }

  // ============== Admin Endpoints ==============

  /**
   * Lấy danh sách đơn hàng (Admin)
   */
  getAdminOrders(params: AdminOrderQueryParams = {}): Observable<PagedResult<AdminOrderDto>> {
    let httpParams = new HttpParams()
      .set('page', (params.page ?? 1).toString())
      .set('pageSize', (params.pageSize ?? 10).toString());

    if (params.status !== null && params.status !== undefined) {
      httpParams = httpParams.set('status', params.status.toString());
    }

    if (params.search) {
      httpParams = httpParams.set('search', params.search);
    }

    return this.http.get<PagedResult<AdminOrderDto>>(this.adminApiUrl, { params: httpParams });
  }

  /**
   * Lấy chi tiết đơn hàng (Admin)
   */
  getAdminOrderDetail(id: number): Observable<AdminOrderDetailDto> {
    return this.http.get<AdminOrderDetailDto>(`${this.adminApiUrl}/${id}`);
  }

  /**
   * Cập nhật trạng thái đơn hàng
   */
  updateOrderStatus(id: number, request: UpdateOrderStatusRequest): Observable<{ message: string; orderId: number; oldStatus: string; newStatus: string; updatedAt: Date }> {
    return this.http.put<{ message: string; orderId: number; oldStatus: string; newStatus: string; updatedAt: Date }>(
      `${this.adminApiUrl}/${id}/status`, 
      request
    );
  }

  /**
   * Lấy danh sách trạng thái đơn hàng
   */
  getOrderStatuses(): Observable<OrderStatusOption[]> {
    return this.http.get<OrderStatusOption[]>(`${this.adminApiUrl}/statuses`);
  }
}
