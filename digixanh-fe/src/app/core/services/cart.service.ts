import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, catchError, map, of, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AddCartItemRequest, CartSummaryDto } from '../models/cart.model';

@Injectable({
  providedIn: 'root'
})
export class CartService {
  private readonly apiUrl = `${environment.apiUrl}/cart`;
  private readonly cartCountSubject = new BehaviorSubject<number>(0);

  readonly cartCount$ = this.cartCountSubject.asObservable();

  constructor(private readonly http: HttpClient) {}

  getCart(): Observable<CartSummaryDto> {
    return this.http.get<CartSummaryDto>(this.apiUrl).pipe(
      tap((summary) => this.cartCountSubject.next(summary.totalQuantity ?? 0))
    );
  }

  addToCart(request: AddCartItemRequest): Observable<CartSummaryDto> {
    return this.http.post<CartSummaryDto>(`${this.apiUrl}/items`, request).pipe(
      tap((summary) => this.cartCountSubject.next(summary.totalQuantity ?? 0))
    );
  }

  refreshCartCount(): Observable<number> {
    return this.getCart().pipe(
      map((summary) => summary.totalQuantity ?? 0),
      catchError(() => {
        this.cartCountSubject.next(0);
        return of(0);
      })
    );
  }

  resetCartCount(): void {
    this.cartCountSubject.next(0);
  }
}