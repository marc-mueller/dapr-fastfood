import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Order } from '../models/order.model';
import { CartItem } from '../models/cart-item.model';

@Injectable({
  providedIn: 'root'
})
export class OrderService {
  private apiUrl = 'api/order';

  constructor(private http: HttpClient) {}

  createOrder(order: Order): Observable<Order> {
    return this.http.post<Order>(`${this.apiUrl}/createOrder`, order);
  }

  getOrder(orderId: string): Observable<Order> {
    return this.http.get<Order>(`${this.apiUrl}/${orderId}`);
  }

  addItem(orderId: string, item: CartItem): Observable<CartItem> {
    return this.http.post<CartItem>(`${this.apiUrl}/addItem/${orderId}`, item);
  }

  removeItem(orderId: string, itemId: string): Observable<CartItem> {
    return this.http.post<CartItem>(`${this.apiUrl}/removeItem/${orderId}`, { itemId });
  }

  confirmOrder(orderId: string): Observable<Order> {
    return this.http.post<Order>(`${this.apiUrl}/confirmOrder/${orderId}`, {});
  }

  confirmPayment(orderId: string): Observable<Order> {
    return this.http.post<Order>(`${this.apiUrl}/confirmPayment/${orderId}`, {});
  }
}
