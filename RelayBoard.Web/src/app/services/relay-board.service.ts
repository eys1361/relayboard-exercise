import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Driver } from '../models/driver.model';
import { Lookups } from '../models/lookups.model';
import { Order } from '../models/order.model';

@Injectable({ providedIn: 'root' })
export class RelayBoardService {
  private readonly baseUrl = '/api';

  constructor(private readonly http: HttpClient) {}

  getOrders(status?: string): Observable<Order[]> {
    const options = status ? { params: { status } } : {};
    return this.http.get<Order[]>(`${this.baseUrl}/orders`, options);
  }

  getDrivers(): Observable<Driver[]> {
    return this.http.get<Driver[]>(`${this.baseUrl}/drivers`);
  }

  getNearbyDrivers(lat: number, lng: number): Observable<Driver[]> {
    return this.http.get<Driver[]>(`${this.baseUrl}/drivers/nearby`, {
      params: { lat: String(lat), lng: String(lng) },
    });
  }

  getLookups(): Observable<Lookups> {
    return this.http.get<Lookups>(`${this.baseUrl}/lookups`);
  }

  assignDriver(orderId: number, driverId: number): Observable<Order> {
    return this.http.post<Order>(`${this.baseUrl}/orders/${orderId}/assign`, {
      driverId,
    });
  }
}
