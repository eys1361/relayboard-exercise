using Microsoft.EntityFrameworkCore;
using RelayBoard.Api.Data;
using RelayBoard.Api.Domain;
using RelayBoard.Api.Dtos;
using RelayBoard.Api.Mapping;

namespace RelayBoard.Api.Services;

public interface IOrderService
{
    Task<IReadOnlyList<OrderDto>> GetOrdersAsync(string? status, CancellationToken cancellationToken = default);
    Task<OrderDto?> GetOrderAsync(int id, CancellationToken cancellationToken = default);
    Task<AssignResult> AssignAsync(int orderId, int driverId, CancellationToken cancellationToken = default);
}

public record AssignResult(bool Found, string? Error, OrderDto? Order)
{
    public static AssignResult NotFound() => new(false, null, null);
    public static AssignResult Fail(string error) => new(true, error, null);
    public static AssignResult Ok(OrderDto order) => new(true, null, order);
}

public class OrderService(RelayBoardContext db) : IOrderService
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

        var now = DateTime.UtcNow;

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
