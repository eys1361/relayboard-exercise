using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace RelayBoard.Api.Tests;

public class OrderApiTests : IClassFixture<RelayBoardApiFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private readonly HttpClient _client;

    public OrderApiTests(RelayBoardApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Get_open_orders_returns_seeded_unassigned_work()
    {
        var orders = await _client.GetFromJsonAsync<List<OrderResponse>>("/api/orders?status=OPEN", JsonOptions);

        Assert.NotNull(orders);
        Assert.Contains(orders, o => o.OrderNumber == "RB-1001");
        Assert.Contains(orders, o => o.RequiredVehicleType == "VAN");
        Assert.DoesNotContain(orders, o => o.Status == "DELIVERED");
    }

    [Fact]
    public async Task Assign_sets_driver_and_status()
    {
        var orders = await _client.GetFromJsonAsync<List<OrderResponse>>("/api/orders?status=OPEN", JsonOptions);
        var order = orders!.First(o => o.OrderNumber == "RB-1002");

        var response = await _client.PostAsJsonAsync(
            $"/api/orders/{order.Id}/assign",
            new { driverId = 2 });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var updated = await response.Content.ReadFromJsonAsync<OrderResponse>(JsonOptions);
        Assert.Equal(2, updated!.AssignedDriverId);
        Assert.Equal("ASSIGNED", updated.Status);
        Assert.Equal("Ben Ortiz", updated.AssignedDriverName);
    }

    [Fact]
    public async Task Assign_rejects_delivered_orders()
    {
        var orders = await _client.GetFromJsonAsync<List<OrderResponse>>("/api/orders", JsonOptions);
        var delivered = orders!.First(o => o.OrderNumber == "RB-1010");

        var response = await _client.PostAsJsonAsync(
            $"/api/orders/{delivered.Id}/assign",
            new { driverId = 1 });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private sealed class OrderResponse
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = "";
        public string Status { get; set; } = "";
        public string? RequiredVehicleType { get; set; }
        public int? AssignedDriverId { get; set; }
        public string? AssignedDriverName { get; set; }
    }
}
