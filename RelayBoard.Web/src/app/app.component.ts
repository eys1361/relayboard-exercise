import { Component, OnInit } from '@angular/core';
import { DatePipe } from '@angular/common';
import { OrderListComponent } from './dispatch/order-list.component';
import { DriverRailComponent } from './dispatch/driver-rail.component';
import { OrderSuggestionsComponent } from './dispatch/order-suggestions.component';
import { RelayBoardService } from './services/relay-board.service';
import { Driver } from './models/driver.model';
import { DriverSuggestion } from './models/driver-suggestion.model';
import { Order } from './models/order.model';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [DatePipe, OrderListComponent, DriverRailComponent, OrderSuggestionsComponent],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css',
})
export class AppComponent implements OnInit {
  orders: Order[] = [];
  drivers: Driver[] = [];
  selectedOrder: Order | null = null;
  suggestions: DriverSuggestion[] | null = null;
  loadingSuggestions = false;
  statusFilter = 'OPEN';
  error: string | null = null;
  busy = false;

  constructor(private readonly api: RelayBoardService) {}

  ngOnInit(): void {
    this.refresh();
  }

  refresh(): void {
    this.error = null;
    this.api.getOrders(this.statusFilter || undefined).subscribe({
      next: (orders) => {
        this.orders = orders;
        if (this.selectedOrder) {
          this.selectedOrder = orders.find((o) => o.id === this.selectedOrder?.id) ?? null;
          this.loadSuggestions();
        }
      },
      error: () => {
        this.error = 'Could not load orders. Is the API running on port 5120?';
      },
    });

    this.api.getDrivers().subscribe({
      next: (drivers) => {
        this.drivers = drivers;
      },
      error: () => {
        this.error = 'Could not load drivers. Is the API running on port 5120?';
      },
    });
  }

  selectOrder(order: Order): void {
    this.selectedOrder = order;
    this.loadSuggestions();
  }

  loadSuggestions(): void {
    if (!this.selectedOrder || this.selectedOrder.status !== 'OPEN') {
      this.suggestions = null;
      this.loadingSuggestions = false;
      return;
    }

    this.loadingSuggestions = true;
    this.api.getDriverSuggestions(this.selectedOrder.id).subscribe({
      next: (suggestions) => {
        this.suggestions = suggestions;
        this.loadingSuggestions = false;
      },
      error: () => {
        this.suggestions = [];
        this.loadingSuggestions = false;
      },
    });
  }

  setFilter(status: string): void {
    this.statusFilter = status;
    this.refresh();
  }

  assign(driver: Driver): void {
    this.assignById(driver.id);
  }

  assignById(driverId: number): void {
    if (!this.selectedOrder) {
      this.error = 'Select an order first.';
      return;
    }

    this.busy = true;
    this.error = null;
    this.api.assignDriver(this.selectedOrder.id, driverId).subscribe({
      next: () => {
        this.busy = false;
        this.refresh();
      },
      error: (err: { error?: { error?: string } }) => {
        this.busy = false;
        this.error = err.error?.error ?? 'Assign failed.';
      },
    });
  }
}
