using RelayBoard.Api.Domain;
using RelayBoard.Api.Dtos;

namespace RelayBoard.Api.Mapping;

public static class DtoMapper
{
    public static AddressDto ToDto(this Address address) => new()
    {
        Id = address.Id,
        Line1 = address.Line1,
        Line2 = address.Line2,
        City = address.City,
        State = address.State,
        PostalCode = address.PostalCode,
        Display = address.Display,
        Latitude = address.Latitude,
        Longitude = address.Longitude,
    };

    public static OrderDto ToDto(this Order order) => new()
    {
        Id = order.Id,
        OrderNumber = order.OrderNumber,
        CustomerName = order.Customer.Name,
        Pickup = order.PickupAddress.ToDto(),
        Dropoff = order.DropoffAddress.ToDto(),
        Status = order.OrderStatus.Code,
        RequiredVehicleType = order.RequiredVehicleType?.Code,
        AssignedDriverId = order.AssignedDriverId,
        AssignedDriverName = order.AssignedDriver?.FullName,
        ReadyAt = order.ReadyAt,
        PickupBy = order.PickupBy,
        DeliverBy = order.DeliverBy,
        Notes = order.Notes,
    };

    public static DriverDto ToDto(
        this Driver driver,
        int activeAssignmentCount,
        IReadOnlyList<RouteStopDto> currentPlan) => new()
    {
        Id = driver.Id,
        FirstName = driver.FirstName,
        LastName = driver.LastName,
        Name = driver.FullName,
        Phone = driver.Phone,
        VehicleType = driver.VehicleType.Code,
        Status = driver.DriverStatus.Code,
        Lat = driver.CurrentLatitude,
        Lng = driver.CurrentLongitude,
        LastLocationAt = driver.LastLocationAt,
        ActiveAssignmentCount = activeAssignmentCount,
        CurrentPlan = currentPlan,
    };

    public static IReadOnlyList<RouteStopDto> ToPlan(IEnumerable<Assignment> activeAssignments)
    {
        var stops = new List<RouteStopDto>();
        var sequence = 1;
        foreach (var assignment in activeAssignments.OrderBy(a => a.StopSequence).ThenBy(a => a.AssignedAt))
        {
            var order = assignment.Order;
            if (order.OrderStatusId != OrderStatusIds.InTransit)
            {
                stops.Add(new RouteStopDto
                {
                    Sequence = sequence++,
                    Kind = "PICKUP",
                    OrderId = order.Id,
                    OrderNumber = order.OrderNumber,
                    Address = order.PickupAddress.Display,
                    Latitude = order.PickupAddress.Latitude,
                    Longitude = order.PickupAddress.Longitude,
                    SlaAt = order.PickupBy,
                });
            }

            stops.Add(new RouteStopDto
            {
                Sequence = sequence++,
                Kind = "DROPOFF",
                OrderId = order.Id,
                OrderNumber = order.OrderNumber,
                Address = order.DropoffAddress.Display,
                Latitude = order.DropoffAddress.Latitude,
                Longitude = order.DropoffAddress.Longitude,
                SlaAt = order.DeliverBy,
            });
        }

        return stops;
    }
}
