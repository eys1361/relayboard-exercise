namespace RelayBoard.Api.Domain;

public class Address
{
    public int Id { get; set; }
    public required string Line1 { get; set; }
    public string? Line2 { get; set; }
    public required string City { get; set; }
    public required string State { get; set; }
    public required string PostalCode { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }

    public string Display => $"{Line1}, {City}, {State} {PostalCode}";
}
