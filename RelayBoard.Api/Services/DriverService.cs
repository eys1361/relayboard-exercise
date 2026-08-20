using Microsoft.EntityFrameworkCore;
using RelayBoard.Api.Data;
using RelayBoard.Api.Domain;
using RelayBoard.Api.Dtos;
using RelayBoard.Api.Mapping;

namespace RelayBoard.Api.Services;

public interface IDriverService
{
    Task<IReadOnlyList<DriverDto>> GetDriversAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DriverDto>> GetNearbyDriversAsync(
        double lat,
        double lng,
        double degreeBox = 0.05,
        CancellationToken cancellationToken = default);
}

public class DriverService(RelayBoardContext db) : IDriverService
{
    public async Task<IReadOnlyList<DriverDto>> GetDriversAsync(CancellationToken cancellationToken = default)
    {
        var drivers = await QueryDrivers().ToListAsync(cancellationToken);
        var plans = await GetActivePlansAsync(cancellationToken);
        return drivers
            .Select(d => d.ToDto(
                plans.GetValueOrDefault(d.Id)?.Count ?? 0,
                DtoMapper.ToPlan(plans.GetValueOrDefault(d.Id) ?? [])))
            .ToList();
    }

    /// <summary>
    /// Distance is already computed in GetNearbyDrivers — reuse it for ranking work.
    /// Uses a lat/lng bounding box (degreeBox defaults to 0.05, Euclidean on the plane).
    /// </summary>
    public async Task<IReadOnlyList<DriverDto>> GetNearbyDriversAsync(
        double lat,
        double lng,
        double degreeBox = 0.05,
        CancellationToken cancellationToken = default)
    {
        var minLat = lat - degreeBox;
        var maxLat = lat + degreeBox;
        var minLng = lng - degreeBox;
        var maxLng = lng + degreeBox;

        var drivers = await QueryDrivers()
            .Where(d =>
                d.CurrentLatitude >= minLat &&
                d.CurrentLatitude <= maxLat &&
                d.CurrentLongitude >= minLng &&
                d.CurrentLongitude <= maxLng)
            .ToListAsync(cancellationToken);

        var plans = await GetActivePlansAsync(cancellationToken);
        return drivers
            .Select(d => d.ToDto(
                plans.GetValueOrDefault(d.Id)?.Count ?? 0,
                DtoMapper.ToPlan(plans.GetValueOrDefault(d.Id) ?? [])))
            .ToList();
    }

    private IQueryable<Domain.Driver> QueryDrivers() =>
        db.Drivers
            .AsNoTracking()
            .Include(d => d.VehicleType)
            .Include(d => d.DriverStatus)
            .OrderBy(d => d.LastName)
            .ThenBy(d => d.FirstName);

    private async Task<Dictionary<int, List<Assignment>>> GetActivePlansAsync(
        CancellationToken cancellationToken)
    {
        var assignments = await db.Assignments
            .AsNoTracking()
            .Where(a => a.UnassignedAt == null)
            .Include(a => a.Order).ThenInclude(o => o.PickupAddress)
            .Include(a => a.Order).ThenInclude(o => o.DropoffAddress)
            .ToListAsync(cancellationToken);

        return assignments
            .GroupBy(a => a.DriverId)
            .ToDictionary(g => g.Key, g => g.ToList());
    }
}
