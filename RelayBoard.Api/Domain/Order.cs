namespace RelayBoard.Api.Domain;

public class Order
{
    public int Id { get; set; }
    public required string OrderNumber { get; set; }
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public int PickupAddressId { get; set; }
    public Address PickupAddress { get; set; } = null!;
    public int DropoffAddressId { get; set; }
    public Address DropoffAddress { get; set; } = null!;
    public int OrderStatusId { get; set; }
    public OrderStatus OrderStatus { get; set; } = null!;
    public int? RequiredVehicleTypeId { get; set; }
    public VehicleType? RequiredVehicleType { get; set; }
    public int? AssignedDriverId { get; set; }
    public Driver? AssignedDriver { get; set; }
    public DateTime ReadyAt { get; set; }
    public DateTime PickupBy { get; set; }
    public DateTime DeliverBy { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }

    public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
}
