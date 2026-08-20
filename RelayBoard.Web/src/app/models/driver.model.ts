export interface RouteStop {
  sequence: number;
  kind: string;
  orderId: number;
  orderNumber: string;
  address: string;
  latitude: number;
  longitude: number;
  slaAt: string;
}

export interface Driver {
  id: number;
  firstName: string;
  lastName: string;
  name: string;
  phone?: string | null;
  vehicleType: string;
  status: string;
  lat: number;
  lng: number;
  lastLocationAt: string;
  activeAssignmentCount: number;
  currentPlan: RouteStop[];
}
