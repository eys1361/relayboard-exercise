using Microsoft.EntityFrameworkCore;
using RelayBoard.Api.Data;
using RelayBoard.Api.Domain;
using RelayBoard.Api.Dtos;
using RelayBoard.Api.Mapping;
using RelayBoard.Api.Planning;

namespace RelayBoard.Api.Services;

public interface IOrderService
{
    Task<IReadOnlyList<OrderDto>> GetOrdersAsync(string? status, CancellationToken cancellationToken = default);
    Task<OrderDto?> GetOrderAsync(int id, CancellationToken cancellationToken = default);
    Task<AssignResult> AssignAsync(int orderId, int driverId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DriverSuggestionDto>?> GetSuggestionsAsync(int id, DateTime? nowOverride = null, CancellationToken cancellationToken = default);
}

public record AssignResult(bool Found, string? Error, OrderDto? Order)
{
    public static AssignResult NotFound() => new(false, null, null);
    public static AssignResult Fail(string error) => new(true, error, null);
    public static AssignResult Ok(OrderDto order) => new(true, null, order);
}

public class OrderService(RelayBoardContext db, TimeProvider timeProvider) : IOrderService
{
    public async Task<IReadOnlyList<OrderDto>> GetOrdersAsync(
        string? status,
        CancellationToken cancellationToken = default)
    {
        var query = QueryOrders();
        if (!string.IsNullOrWhiteSpace(status))
        {
            var code = status.Trim().ToUpperInvariant();
            query = query.Where(o => o.OrderStatus.Code == code);
        }

        var orders = await query
            .OrderBy(o => o.ReadyAt)
            .ThenBy(o => o.OrderNumber)
            .ToListAsync(cancellationToken);

        return orders.Select(o => o.ToDto()).ToList();
    }

    public async Task<OrderDto?> GetOrderAsync(int id, CancellationToken cancellationToken = default)
    {
        var order = await QueryOrders().FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
        return order?.ToDto();
    }

    public async Task<AssignResult> AssignAsync(
        int orderId,
        int driverId,
        CancellationToken cancellationToken = default)
    {
        var order = await db.Orders
            .Include(o => o.OrderStatus)
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);

        if (order is null)
        {
            return AssignResult.NotFound();
        }

        if (order.OrderStatusId is OrderStatusIds.Delivered or OrderStatusIds.Cancelled)
        {
            return AssignResult.Fail("Cannot assign a delivered or cancelled order.");
        }

        var driver = await db.Drivers.FirstOrDefaultAsync(d => d.Id == driverId, cancellationToken);
        if (driver is null)
        {
            return AssignResult.Fail("Driver was not found.");
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;

        var previous = await db.Assignments
            .Where(a => a.OrderId == orderId && a.UnassignedAt == null)
            .ToListAsync(cancellationToken);

        foreach (var assignment in previous)
        {
            assignment.UnassignedAt = now;
        }

        if (order.AssignedDriverId is int previousDriverId && previousDriverId != driverId)
        {
            await ReleaseDriverIfIdleAsync(previousDriverId, orderId, now, cancellationToken);
        }

        var nextSequence = await db.Assignments
            .Where(a => a.DriverId == driverId && a.UnassignedAt == null)
            .Select(a => (int?)a.StopSequence)
            .MaxAsync(cancellationToken) ?? 0;

        db.Assignments.Add(new Assignment
        {
            OrderId = orderId,
            DriverId = driverId,
            AssignedAt = now,
            StopSequence = nextSequence + 1,
        });

        order.AssignedDriverId = driverId;
        order.OrderStatusId = OrderStatusIds.Assigned;
        driver.DriverStatusId = DriverStatusIds.OnJob;

        await db.SaveChangesAsync(cancellationToken);

        var updated = await QueryOrders().FirstAsync(o => o.Id == orderId, cancellationToken);
        return AssignResult.Ok(updated.ToDto());
    }

    public async Task<IReadOnlyList<DriverSuggestionDto>?> GetSuggestionsAsync(
        int id,
        DateTime? nowOverride = null,
        CancellationToken cancellationToken = default)
    {
        var order = await db.Orders
            .AsNoTracking()
            .Include(o => o.PickupAddress)
            .Include(o => o.DropoffAddress)
            .Include(o => o.RequiredVehicleType)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

        if (order is null)
        {
            return null;
        }

        var now = nowOverride ?? timeProvider.GetUtcNow().UtcDateTime;

        var driverQuery = db.Drivers
            .AsNoTracking()
            .Include(d => d.VehicleType)
            .Include(d => d.DriverStatus)
            .Where(d => d.DriverStatus.Code == "AVAILABLE" || d.DriverStatus.Code == "ON_JOB");

        if (order.RequiredVehicleType is not null)
        {
            driverQuery = driverQuery.Where(d => d.VehicleTypeId == order.RequiredVehicleTypeId);
        }

        var qualifyingDrivers = await driverQuery.ToListAsync(cancellationToken);
        if (qualifyingDrivers.Count == 0)
        {
            return Array.Empty<DriverSuggestionDto>();
        }

        var activeAssignments = await db.Assignments
            .AsNoTracking()
            .Where(a => a.UnassignedAt == null)
            .Include(a => a.Order).ThenInclude(o => o.PickupAddress)
            .Include(a => a.Order).ThenInclude(o => o.DropoffAddress)
            .ToListAsync(cancellationToken);

        var plansByDriver = activeAssignments
            .GroupBy(a => a.DriverId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var pickupStop = new PlanStop
        {
            Latitude = order.PickupAddress.Latitude,
            Longitude = order.PickupAddress.Longitude,
            Deadline = order.PickupBy,
            Kind = StopKind.Pickup,
            OrderId = order.Id,
        };

        var dropoffStop = new PlanStop
        {
            Latitude = order.DropoffAddress.Latitude,
            Longitude = order.DropoffAddress.Longitude,
            Deadline = order.DeliverBy,
            Kind = StopKind.Dropoff,
            OrderId = order.Id,
        };

        var suggestions = new List<DriverSuggestionDto>();

        foreach (var driver in qualifyingDrivers)
        {
            var driverAssignments = plansByDriver.GetValueOrDefault(driver.Id) ?? [];
            var existingStops = BuildPlanStops(driverAssignments);

            var milesToPickup = Haversine.MilesBetween(
                driver.CurrentLatitude,
                driver.CurrentLongitude,
                order.PickupAddress.Latitude,
                order.PickupAddress.Longitude);

            var result = InsertionPlanner.Plan(
                driver.CurrentLatitude,
                driver.CurrentLongitude,
                now,
                existingStops,
                pickupStop,
                dropoffStop);

            var slaSlipMinutes = (int)Math.Max(0, Math.Round(result.SlipMinutes));
            var roundedMilesToPickup = Math.Round(milesToPickup, 1);
            var roundedExtraMiles = Math.Round(result.ExtraMiles, 1);
            var reason = FormatReason(driver.DriverStatus.Code, roundedMilesToPickup, roundedExtraMiles, slaSlipMinutes);

            suggestions.Add(new DriverSuggestionDto
            {
                DriverId = driver.Id,
                DriverName = driver.FullName,
                VehicleType = driver.VehicleType.Code,
                MilesToPickup = roundedMilesToPickup,
                ExtraMiles = roundedExtraMiles,
                SlaSlipMinutes = slaSlipMinutes,
                Reason = reason,
            });
        }

        return suggestions
            .OrderBy(s => s.SlaSlipMinutes)
            .ThenBy(s => s.ExtraMiles)
            .ThenBy(s => s.MilesToPickup)
            .ThenBy(s => s.DriverName)
            .Take(3)
            .ToList();
    }

    private static List<PlanStop> BuildPlanStops(IEnumerable<Assignment> activeAssignments)
    {
        var stops = new List<PlanStop>();
        foreach (var assignment in activeAssignments.OrderBy(a => a.StopSequence).ThenBy(a => a.AssignedAt))
        {
            var order = assignment.Order;
            if (order.OrderStatusId != OrderStatusIds.InTransit)
            {
                stops.Add(new PlanStop
                {
                    Latitude = order.PickupAddress.Latitude,
                    Longitude = order.PickupAddress.Longitude,
                    Deadline = order.PickupBy,
                    Kind = StopKind.Pickup,
                    OrderId = order.Id,
                });
            }

            stops.Add(new PlanStop
            {
                Latitude = order.DropoffAddress.Latitude,
                Longitude = order.DropoffAddress.Longitude,
                Deadline = order.DeliverBy,
                Kind = StopKind.Dropoff,
                OrderId = order.Id,
            });
        }

        return stops;
    }

    private static string FormatReason(string statusCode, double milesToPickup, double extraMiles, int slipMinutes)
    {
        var statusLabel = statusCode == "AVAILABLE" ? "Available" : "On job";
        if (slipMinutes == 0)
        {
            return $"{statusLabel}, 0 min SLA slip ({milesToPickup:F1} mi to pickup)";
        }
        return $"{statusLabel}, +{slipMinutes} min SLA slip ({extraMiles:F1} extra mi)";
    }

    private IQueryable<Order> QueryOrders() =>
        db.Orders
            .AsNoTracking()
            .Include(o => o.Customer)
            .Include(o => o.PickupAddress)
            .Include(o => o.DropoffAddress)
            .Include(o => o.OrderStatus)
            .Include(o => o.RequiredVehicleType)
            .Include(o => o.AssignedDriver);

    private async Task ReleaseDriverIfIdleAsync(
        int driverId,
        int excludingOrderId,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var stillBusy = await db.Assignments.AnyAsync(
            a => a.DriverId == driverId
                 && a.UnassignedAt == null
                 && a.OrderId != excludingOrderId,
            cancellationToken);

        if (stillBusy)
        {
            return;
        }

        var driver = await db.Drivers.FirstAsync(d => d.Id == driverId, cancellationToken);
        driver.DriverStatusId = DriverStatusIds.Available;
        driver.LastLocationAt = now;
    }
}
