namespace RelayBoard.Api.Dtos;

public record DriverSuggestionDto
{
    public required int DriverId { get; init; }
    public required string DriverName { get; init; }
    public required string VehicleType { get; init; }
    public required double MilesToPickup { get; init; }
    public required double ExtraMiles { get; init; }
    public required int SlaSlipMinutes { get; init; }
    public required string Reason { get; init; }
}
