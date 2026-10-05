namespace RelayBoard.Api.Planning;

/// <summary>
/// Pure insertion search over a driver's existing stop sequence.
/// Lateness is measured at arrival (before dwell).
/// Slip is total lateness of the candidate plan minus baseline lateness,
/// so lateness already present in the baseline does not count as slip,
/// and lateness on the newly inserted stops does.
/// </summary>
public static class InsertionPlanner
{
    public static InsertionResult Plan(
        double driverLatitude,
        double driverLongitude,
        DateTime now,
        IReadOnlyList<PlanStop> existingStops,
        PlanStop pickup,
        PlanStop dropoff)
    {
        ArgumentNullException.ThrowIfNull(existingStops);
        ArgumentNullException.ThrowIfNull(pickup);
        ArgumentNullException.ThrowIfNull(dropoff);

        var baseline = Evaluate(driverLatitude, driverLongitude, now, existingStops);
        var n = existingStops.Count;

        InsertionResult? best = null;

        for (var pickupAt = 0; pickupAt <= n; pickupAt++)
        {
            for (var dropoffAt = pickupAt; dropoffAt <= n; dropoffAt++)
            {
                var candidate = BuildRoute(existingStops, pickup, dropoff, pickupAt, dropoffAt);
                var evaluated = Evaluate(driverLatitude, driverLongitude, now, candidate);

                var slipMinutes = evaluated.TotalLatenessMinutes - baseline.TotalLatenessMinutes;
                var extraMiles = evaluated.TotalMiles - baseline.TotalMiles;

                var result = new InsertionResult(slipMinutes, extraMiles, pickupAt, dropoffAt);
                if (best is null || IsBetter(result, best))
                {
                    best = result;
                }
            }
        }

        return best!;
    }

    private static bool IsBetter(InsertionResult candidate, InsertionResult incumbent)
    {
        if (candidate.SlipMinutes < incumbent.SlipMinutes)
        {
            return true;
        }

        if (candidate.SlipMinutes > incumbent.SlipMinutes)
        {
            return false;
        }

        return candidate.ExtraMiles < incumbent.ExtraMiles;
    }

    private static List<PlanStop> BuildRoute(
        IReadOnlyList<PlanStop> existing,
        PlanStop pickup,
        PlanStop dropoff,
        int pickupAt,
        int dropoffAt)
    {
        var route = new List<PlanStop>(existing.Count + 2);
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

    private static RouteEvaluation Evaluate(
        double driverLatitude,
        double driverLongitude,
        DateTime now,
        IReadOnlyList<PlanStop> stops)
    {
        var totalMiles = 0.0;
        var totalLatenessMinutes = 0.0;
        var lat = driverLatitude;
        var lng = driverLongitude;
        var time = now;

        foreach (var stop in stops)
        {
            var legMiles = Haversine.MilesBetween(lat, lng, stop.Latitude, stop.Longitude);
            totalMiles += legMiles;
            time = time.AddMinutes(TravelModel.TravelMinutes(legMiles));

            // Lateness at arrival (before dwell).
            var lateness = Math.Max(0, (time - stop.Deadline).TotalMinutes);
            totalLatenessMinutes += lateness;

            time = time.AddMinutes(TravelModel.DwellMinutesPerStop);
            lat = stop.Latitude;
            lng = stop.Longitude;
        }

        return new RouteEvaluation(totalMiles, totalLatenessMinutes);
    }

    private readonly record struct RouteEvaluation(double TotalMiles, double TotalLatenessMinutes);
}

public sealed record InsertionResult(
    double SlipMinutes,
    double ExtraMiles,
    int PickupInsertAt,
    int DropoffInsertAt);
