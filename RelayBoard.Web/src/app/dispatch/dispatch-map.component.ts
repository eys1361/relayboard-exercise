import {
  Component,
  ElementRef,
  EventEmitter,
  Input,
  OnChanges,
  OnDestroy,
  AfterViewInit,
  Output,
  SimpleChanges,
  ViewChild,
} from '@angular/core';
import * as L from 'leaflet';
import { Driver } from '../models/driver.model';
import { DriverSuggestion } from '../models/driver-suggestion.model';
import { Order } from '../models/order.model';

@Component({
  selector: 'app-dispatch-map',
  standalone: true,
  imports: [],
  templateUrl: './dispatch-map.component.html',
  styleUrl: './dispatch-map.component.css',
})
export class DispatchMapComponent implements AfterViewInit, OnChanges, OnDestroy {
  @ViewChild('mapContainer', { static: true }) mapContainer!: ElementRef<HTMLDivElement>;

  @Input() orders: Order[] = [];
  @Input() drivers: Driver[] = [];
  @Input() selectedOrder: Order | null = null;
  @Input() suggestions: DriverSuggestion[] | null = null;

  @Output() orderSelected = new EventEmitter<Order>();

  private map: L.Map | null = null;
  private markerGroup: L.LayerGroup | null = null;

  ngAfterViewInit(): void {
    this.initMap();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (this.map) {
      this.updateMarkers();
    }
  }

  ngOnDestroy(): void {
    if (this.map) {
      this.map.remove();
      this.map = null;
    }
  }

  private initMap(): void {
    if (this.map) return;

    // Default center NYC
    this.map = L.map(this.mapContainer.nativeElement, {
      center: [40.7484, -73.9857],
      zoom: 12,
      zoomControl: true,
    });

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
      maxZoom: 19,
      attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>',
    }).addTo(this.map);

    this.markerGroup = L.layerGroup().addTo(this.map);
    this.updateMarkers();
  }

  private updateMarkers(): void {
    if (!this.map || !this.markerGroup) return;

    this.markerGroup.clearLayers();
    const bounds: L.LatLngExpression[] = [];

    // Map suggestions by driverId for fast lookup
    const suggestionRankMap = new Map<number, { rank: number; suggestion: DriverSuggestion }>();
    if (this.suggestions) {
      this.suggestions.forEach((s, idx) => {
        suggestionRankMap.set(s.driverId, { rank: idx + 1, suggestion: s });
      });
    }

    // 1. Plot Open Order Pickups
    const openOrders = this.orders.filter((o) => o.status === 'OPEN');
    for (const order of openOrders) {
      const isSelected = this.selectedOrder?.id === order.id;
      const lat = order.pickup.latitude;
      const lng = order.pickup.longitude;
      bounds.push([lat, lng]);

      const html = `
        <div class="map-marker pickup-marker ${isSelected ? 'selected' : ''}">
          <span class="marker-icon">📦</span>
          <span class="marker-label">${order.orderNumber}</span>
        </div>
      `;

      const icon = L.divIcon({
        className: 'custom-div-icon',
        html: html,
        iconSize: [80, 28],
        iconAnchor: [40, 14],
      });

      const marker = L.marker([lat, lng], { icon });
      marker.on('click', () => this.orderSelected.emit(order));
      marker.addTo(this.markerGroup);
    }

    // 2. Plot Selected Order Dropoff
    if (this.selectedOrder) {
      const dropoffLat = this.selectedOrder.dropoff.latitude;
      const dropoffLng = this.selectedOrder.dropoff.longitude;
      bounds.push([dropoffLat, dropoffLng]);

      const html = `
        <div class="map-marker dropoff-marker">
          <span class="marker-icon">📍</span>
          <span class="marker-label">Dropoff</span>
        </div>
      `;

      const icon = L.divIcon({
        className: 'custom-div-icon',
        html: html,
        iconSize: [80, 28],
        iconAnchor: [40, 14],
      });

      const marker = L.marker([dropoffLat, dropoffLng], { icon });
      marker.bindTooltip(`Dropoff for ${this.selectedOrder.orderNumber}: ${this.selectedOrder.dropoff.display}`, {
        direction: 'top',
      });
      marker.addTo(this.markerGroup);
    }

    // 3. Plot Drivers (AVAILABLE & ON_JOB, exclude OFF_DUTY)
    const activeDrivers = this.drivers.filter((d) => d.status !== 'OFF_DUTY');
    for (const driver of activeDrivers) {
      const lat = driver.lat;
      const lng = driver.lng;
      bounds.push([lat, lng]);

      const suggestion = suggestionRankMap.get(driver.id);
      const isAvailable = driver.status === 'AVAILABLE';
      const statusClass = isAvailable ? 'available' : 'on-job';

      let html = '';
      if (suggestion) {
        html = `
          <div class="map-marker driver-marker suggested-driver rank-${suggestion.rank}">
            <span class="rank-badge">#${suggestion.rank}</span>
            <span class="marker-label">${driver.firstName}</span>
            <span class="vehicle-badge">${driver.vehicleType}</span>
          </div>
        `;
      } else {
        html = `
          <div class="map-marker driver-marker ${statusClass}">
            <span class="marker-icon">${isAvailable ? '🟢' : '⚪'}</span>
            <span class="marker-label">${driver.firstName}</span>
          </div>
        `;
      }

      const icon = L.divIcon({
        className: 'custom-div-icon',
        html: html,
        iconSize: [90, 30],
        iconAnchor: [45, 15],
      });

      const marker = L.marker([lat, lng], { icon });
      const tooltipText = suggestion
        ? `Driver #${suggestion.rank}: ${driver.name} (${driver.vehicleType}) - ${suggestion.reason}`
        : `${driver.name} (${driver.vehicleType}) - ${driver.status}`;
      marker.bindTooltip(tooltipText, { direction: 'top' });
      marker.addTo(this.markerGroup);
    }

    // Fit map bounds to all plotted markers
    if (bounds.length > 0) {
      this.map.fitBounds(L.latLngBounds(bounds), {
        padding: [40, 40],
        maxZoom: 15,
      });
    }
  }
}
