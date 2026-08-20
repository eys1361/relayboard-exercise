namespace RelayBoard.Api.Domain;

public class DriverStatus
{
    public int Id { get; set; }
    public required string Code { get; set; }
    public required string Name { get; set; }

    public ICollection<Driver> Drivers { get; set; } = new List<Driver>();
}
