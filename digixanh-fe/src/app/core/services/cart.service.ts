import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, catchError, finalize, map, of, shareReplay, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AddCartItemRequest, CartSummaryDto, UpdateCartItemQuantityRequest } from '../models/cart.model';

@Injectable({
  providedIn: 'root'
})
export class CartService {
  private readonly apiUrl = `${environment.apiUrl}/cart`;
  private readonly cartCountSubject = new BehaviorSubject<number>(0);
  private readonly refreshTtlMs = 30_000;
  private lastRefreshAt = 0;
  private refreshInFlight$: Observable<number> | null = null;

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

  updateCartItemQuantity(cartItemId: number, request: UpdateCartItemQuantityRequest): Observable<CartSummaryDto> {
    return this.http.put<CartSummaryDto>(`${this.apiUrl}/items/${cartItemId}`, request).pipe(
      tap((summary) => this.cartCountSubject.next(summary.totalQuantity ?? 0))
    );
  }

  removeCartItem(cartItemId: number): Observable<CartSummaryDto> {
    return this.http.delete<CartSummaryDto>(`${this.apiUrl}/items/${cartItemId}`).pipe(
      tap((summary) => this.cartCountSubject.next(summary.totalQuantity ?? 0))
    );
  }

  refreshCartCount(force = false): Observable<number> {
    const now = Date.now();
    const cachedCount = this.cartCountSubject.getValue();

    if (!force && this.lastRefreshAt > 0 && now - this.lastRefreshAt < this.refreshTtlMs) {
      return of(cachedCount);
    }

    if (this.refreshInFlight$) {
      return this.refreshInFlight$;
    }

    const request$ = this.getCart().pipe(
      map((summary) => summary.totalQuantity ?? 0),
      tap(() => {
        this.lastRefreshAt = Date.now();
      }),
      catchError(() => {
        this.cartCountSubject.next(0);
        return of(0);
      }),
      finalize(() => {
        this.refreshInFlight$ = null;
      }),
      shareReplay({ bufferSize: 1, refCount: false })
    );

    this.refreshInFlight$ = request$;
    return request$;
  }

  resetCartCount(): void {
    this.cartCountSubject.next(0);
    this.lastRefreshAt = 0;
    this.refreshInFlight$ = null;
  }
}