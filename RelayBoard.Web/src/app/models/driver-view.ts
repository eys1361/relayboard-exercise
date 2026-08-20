import { Driver } from './driver.model';

/**
 * Preferred view model for dispatcher map and suggestion work.
 * Coordinates use geographic field names (latitude / longitude).
 */
export interface DriverView {
  id: number;
  displayName: string;
  vehicleType: string;
  status: string;
  latitude: number;
  longitude: number;
  activeAssignmentCount: number;
}

export function toDriverView(driver: Driver): DriverView {
  return {
    id: driver.id,
    displayName: driver.name,
    vehicleType: driver.vehicleType,
    status: driver.status,
    latitude: (driver as unknown as DriverView).latitude,
    longitude: (driver as unknown as DriverView).longitude,
    activeAssignmentCount: driver.activeAssignmentCount,
  };
}
