using RelayBoard.Api.Planning;

namespace RelayBoard.Api.Tests;

public class InsertionPlannerTests
{
    // Empire State Building → Wall Street (approx).
    private const double EmpireLat = 40.7484;
    private const double EmpireLng = -73.9857;
    private const double WallLat = 40.7074;
    private const double WallLng = -74.0113;
    private const double EmpireToWallMiles = 3.15;

    [Fact]
    public void Haversine_returns_known_nyc_distance_within_tolerance()
    {
        var miles = Haversine.MilesBetween(EmpireLat, EmpireLng, WallLat, WallLng);

        Assert.InRange(miles, EmpireToWallMiles - 0.05, EmpireToWallMiles + 0.05);
    }

    [Fact]
    public void Idle_driver_extra_miles_equals_driver_to_pickup_to_dropoff()
    {
        var now = new DateTime(2026, 8, 18, 14, 0, 0, DateTimeKind.Utc);
        var driverLat = 40.7589;
        var driverLng = -73.9851;
        var pickup = Stop(40.7580, -73.9855, now.AddHours(2), StopKind.Pickup, orderId: 100);
        var dropoff = Stop(40.7074, -74.0113, now.AddHours(3), StopKind.Dropoff, orderId: 100);

        var result = InsertionPlanner.Plan(driverLat, driverLng, now, [], pickup, dropoff);

        var expected =
            Haversine.MilesBetween(driverLat, driverLng, pickup.Latitude, pickup.Longitude)
            + Haversine.MilesBetween(pickup.Latitude, pickup.Longitude, dropoff.Latitude, dropoff.Longitude);

        Assert.Equal(0, result.PickupInsertAt);
        Assert.Equal(0, result.DropoffInsertAt);
        Assert.Equal(expected, result.ExtraMiles, precision: 6);
        Assert.Equal(0, result.SlipMinutes, precision: 6);
    }

    [Fact]
    public void Existing_stops_are_never_reordered()
    {
        var now = new DateTime(2026, 8, 18, 14, 0, 0, DateTimeKind.Utc);
        var existing = new[]
        {
            Stop(40.75, -73.99, now.AddHours(4), StopKind.Pickup, orderId: 1),
            Stop(40.74, -73.98, now.AddHours(5), StopKind.Dropoff, orderId: 1),
            Stop(40.73, -73.97, now.AddHours(6), StopKind.Pickup, orderId: 2),
            Stop(40.72, -73.96, now.AddHours(7), StopKind.Dropoff, orderId: 2),
        };
        var pickup = Stop(40.755, -73.988, now.AddHours(3), StopKind.Pickup, orderId: 99);
        var dropoff = Stop(40.745, -73.978, now.AddHours(3.5), StopKind.Dropoff, orderId: 99);

        var result = InsertionPlanner.Plan(40.76, -73.99, now, existing, pickup, dropoff);

        var route = BuildRoute(existing, pickup, dropoff, result.PickupInsertAt, result.DropoffInsertAt);
        var existingInRoute = route.Where(s => s.OrderId != 99).ToList();

        Assert.Equal(existing.Select(s => (s.OrderId, s.Kind)), existingInRoute.Select(s => (s.OrderId, s.Kind)));
    }

    [Fact]
    public void Baseline_lateness_does_not_count_as_slip()
    {
        var now = new DateTime(2026, 8, 18, 14, 0, 0, DateTimeKind.Utc);
        // Existing stop is already late even on the baseline (deadline in the past).
        var existing = new[]
        {
            Stop(40.7484, -73.9857, now.AddMinutes(-30), StopKind.Dropoff, orderId: 1),
        };
        // New order is on the way and has generous deadlines — does not add delay beyond baseline.
        var pickup = Stop(40.75, -73.986, now.AddHours(2), StopKind.Pickup, orderId: 99);
        var dropoff = Stop(40.749, -73.9858, now.AddHours(3), StopKind.Dropoff, orderId: 99);

        var result = InsertionPlanner.Plan(40.7589, -73.9851, now, existing, pickup, dropoff);

        Assert.Equal(0, result.SlipMinutes, precision: 6);
    }

    [Fact]
    public void Prefers_earlier_insertion_when_appending_would_cause_slip()
    {
        var now = new DateTime(2026, 8, 18, 14, 0, 0, DateTimeKind.Utc);
        var driverLat = 40.7589;
        var driverLng = -73.9851;

        // Existing stop is far away but has plenty of slack if done after a short local trip.
        var existing = new[]
        {
            Stop(40.6892, -73.9857, now.AddHours(5), StopKind.Dropoff, orderId: 1),
        };

        // New order is near the driver with a tight deadline — appending after the far stop misses it.
        var pickup = Stop(40.7580, -73.9855, now.AddMinutes(20), StopKind.Pickup, orderId: 99);
        var dropoff = Stop(40.7575, -73.9860, now.AddMinutes(40), StopKind.Dropoff, orderId: 99);

        var result = InsertionPlanner.Plan(driverLat, driverLng, now, existing, pickup, dropoff);

        Assert.Equal(0, result.SlipMinutes, precision: 6);
        Assert.Equal(0, result.PickupInsertAt);
        Assert.Equal(0, result.DropoffInsertAt);

        // Sanity: appending at the end would slip.
        var appendRoute = BuildRoute(existing, pickup, dropoff, pickupAt: 1, dropoffAt: 1);
        var appendEval = EvaluateLateness(driverLat, driverLng, now, appendRoute);
        var baselineEval = EvaluateLateness(driverLat, driverLng, now, existing);
        Assert.True(appendEval - baselineEval > 0);
    }

    private static PlanStop Stop(
        double lat,
        double lng,
        DateTime deadline,
        StopKind kind,
        int orderId) => new()
    {
        Latitude = lat,
        Longitude = lng,
        Deadline = deadline,
        Kind = kind,
        OrderId = orderId,
    };

    private static List<PlanStop> BuildRoute(
        IReadOnlyList<PlanStop> existing,
        PlanStop pickup,
        PlanStop dropoff,
        int pickupAt,
        int dropoffAt)
    {
        var route = new List<PlanStop>();
        for (var i = 0; i < existing.Count; i++)
        {
            if (i == pickupAt)
            {
                route.Add(pickup);
            }

            if (i == dropoffAt)
            {
                route.Add(dropoff);
            }

            route.Add(existing[i]);
        }

        if (pickupAt == existing.Count)
        {
            route.Add(pickup);
        }

        if (dropoffAt == existing.Count)
        {
            route.Add(dropoff);
        }

        return route;
    }

    private static double EvaluateLateness(
        double driverLat,
        double driverLng,
        DateTime now,
        IReadOnlyList<PlanStop> stops)
    {
        var total = 0.0;
        var lat = driverLat;
        var lng = driverLng;
        var time = now;
        foreach (var stop in stops)
        {
            var miles = Haversine.MilesBetween(lat, lng, stop.Latitude, stop.Longitude);
            time = time.AddMinutes(TravelModel.TravelMinutes(miles));
            total += Math.Max(0, (time - stop.Deadline).TotalMinutes);
            time = time.AddMinutes(TravelModel.DwellMinutesPerStop);
            lat = stop.Latitude;
            lng = stop.Longitude;
        }

        return total;
    }
}
