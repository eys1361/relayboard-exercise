export interface Lookup {
  id: number;
  code: string;
  name: string;
}

export interface Lookups {
  vehicleTypes: Lookup[];
  driverStatuses: Lookup[];
  orderStatuses: Lookup[];
}
