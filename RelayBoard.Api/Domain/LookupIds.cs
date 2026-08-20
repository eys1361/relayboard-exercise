namespace RelayBoard.Api.Domain;

public static class VehicleTypeIds
{
    public const int Bike = 1;
    public const int Car = 2;
    public const int Van = 3;
}

public static class DriverStatusIds
{
    public const int OffDuty = 1;
    public const int Available = 2;
    public const int OnJob = 3;
}

public static class OrderStatusIds
{
    public const int Open = 1;
    public const int Assigned = 2;
    public const int InTransit = 3;
    public const int Delivered = 4;
    public const int Cancelled = 5;
}
