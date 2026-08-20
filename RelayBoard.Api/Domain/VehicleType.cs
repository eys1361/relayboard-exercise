namespace RelayBoard.Api.Domain;

public class VehicleType
{
    public int Id { get; set; }
    public required string Code { get; set; }
    public required string Name { get; set; }

    public ICollection<Driver> Drivers { get; set; } = new List<Driver>();
    public ICollection<Order> RequiredByOrders { get; set; } = new List<Order>();
}
