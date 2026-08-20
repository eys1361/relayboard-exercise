namespace RelayBoard.Api.Dtos;

public class AddressDto
{
    public int Id { get; set; }
    public required string Line1 { get; set; }
    public string? Line2 { get; set; }
    public required string City { get; set; }
    public required string State { get; set; }
    public required string PostalCode { get; set; }
    public required string Display { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}

public class OrderDto
{
    public int Id { get; set; }
    public required string OrderNumber { get; set; }
    public required string CustomerName { get; set; }
    public required AddressDto Pickup { get; set; }
    public required AddressDto Dropoff { get; set; }
    public required string Status { get; set; }
    public string? RequiredVehicleType { get; set; }
    public int? AssignedDriverId { get; set; }
    public string? AssignedDriverName { get; set; }
    public DateTime ReadyAt { get; set; }
    public DateTime PickupBy { get; set; }
    public DateTime DeliverBy { get; set; }
    public string? Notes { get; set; }
}

public class RouteStopDto
{
    public int Sequence { get; set; }
    public required string Kind { get; set; }
    public int OrderId { get; set; }
    public required string OrderNumber { get; set; }
    public required string Address { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public DateTime SlaAt { get; set; }
}

public class DriverDto
{
    public int Id { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Name { get; set; }
    public string? Phone { get; set; }
    public required string VehicleType { get; set; }
    public required string Status { get; set; }
    public double Lat { get; set; }
    public double Lng { get; set; }
    public DateTime LastLocationAt { get; set; }
    public int ActiveAssignmentCount { get; set; }
    public IReadOnlyList<RouteStopDto> CurrentPlan { get; set; } = [];
}

public class AssignDriverRequest
{
    public int DriverId { get; set; }
}

public class LookupDto
{
    public int Id { get; set; }
    public required string Code { get; set; }
    public required string Name { get; set; }
}

public class LookupsDto
{
    public required IReadOnlyList<LookupDto> VehicleTypes { get; set; }
    public required IReadOnlyList<LookupDto> DriverStatuses { get; set; }
    public required IReadOnlyList<LookupDto> OrderStatuses { get; set; }
}
