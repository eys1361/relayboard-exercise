import { Component, EventEmitter, Input, Output } from '@angular/core';
import { DriverSuggestion } from '../models/driver-suggestion.model';

@Component({
  selector: 'app-order-suggestions',
  standalone: true,
  imports: [],
  templateUrl: './order-suggestions.component.html',
  styleUrl: './order-suggestions.component.css',
})
export class OrderSuggestionsComponent {
  @Input() suggestions: DriverSuggestion[] | null = null;
  @Input() loading = false;
  @Input() busy = false;
  @Output() assignDriver = new EventEmitter<number>();
}
