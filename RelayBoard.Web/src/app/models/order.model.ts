export interface Address {
  id: number;
  line1: string;
  line2?: string | null;
  city: string;
  state: string;
  postalCode: string;
  display: string;
  latitude: number;
  longitude: number;
}

export interface Order {
  id: number;
  orderNumber: string;
  customerName: string;
  pickup: Address;
  dropoff: Address;
  status: string;
  requiredVehicleType?: string | null;
  assignedDriverId?: number | null;
  assignedDriverName?: string | null;
  readyAt: string;
  pickupBy: string;
  deliverBy: string;
  notes?: string | null;
}
