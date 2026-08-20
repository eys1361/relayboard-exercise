import { Component, EventEmitter, Input, Output } from '@angular/core';
import { DatePipe } from '@angular/common';
import { Order } from '../models/order.model';

@Component({
  selector: 'app-order-list',
  imports: [DatePipe],
  templateUrl: './order-list.component.html',
  styleUrl: './order-list.component.css',
})
export class OrderListComponent {
  @Input({ required: true }) orders: Order[] = [];
  @Input() selectedOrderId: number | null = null;
  @Output() readonly orderSelected = new EventEmitter<Order>();
}
