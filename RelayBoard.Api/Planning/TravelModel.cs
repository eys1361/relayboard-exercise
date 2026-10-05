namespace RelayBoard.Api.Planning;

public static class TravelModel
{
    public const double MinutesPerMile = 3;
    public const double DwellMinutesPerStop = 8;

    public static double TravelMinutes(double miles) => miles * MinutesPerMile;
}
