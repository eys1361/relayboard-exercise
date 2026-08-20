namespace RelayBoard.Api.Domain;

public class OrderStatus
{
    public int Id { get; set; }
    public required string Code { get; set; }
    public required string Name { get; set; }

    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
