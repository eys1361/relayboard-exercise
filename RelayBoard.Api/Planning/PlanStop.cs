namespace RelayBoard.Api.Planning;

public enum StopKind
{
    Pickup,
    Dropoff,
}

public sealed class PlanStop
{
    public required double Latitude { get; init; }
    public required double Longitude { get; init; }
    public required DateTime Deadline { get; init; }
    public required StopKind Kind { get; init; }
    public required int OrderId { get; init; }
}
