namespace RelayBoard.Api.Domain;

public class Driver
{
    public int Id { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public string? Phone { get; set; }
    public int VehicleTypeId { get; set; }
    public VehicleType VehicleType { get; set; } = null!;
    public int DriverStatusId { get; set; }
    public DriverStatus DriverStatus { get; set; } = null!;
    public double CurrentLatitude { get; set; }
    public double CurrentLongitude { get; set; }
    public DateTime LastLocationAt { get; set; }

    public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
    public ICollection<Order> AssignedOrders { get; set; } = new List<Order>();

    public string FullName => $"{FirstName} {LastName}";
}
