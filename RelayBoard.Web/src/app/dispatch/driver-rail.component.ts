import { Component, EventEmitter, Input, Output } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { Driver } from '../models/driver.model';

@Component({
  selector: 'app-driver-rail',
  imports: [DecimalPipe],
  templateUrl: './driver-rail.component.html',
  styleUrl: './driver-rail.component.css',
})
export class DriverRailComponent {
  @Input({ required: true }) drivers: Driver[] = [];
  @Input() canAssign = false;
  @Input() busy = false;
  @Output() readonly assign = new EventEmitter<Driver>();
}
