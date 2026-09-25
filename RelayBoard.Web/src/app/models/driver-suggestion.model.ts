export interface DriverSuggestion {
  driverId: number;
  driverName: string;
  vehicleType: string;
  milesToPickup: number;
  extraMiles: number;
  slaSlipMinutes: number;
  reason: string;
}
