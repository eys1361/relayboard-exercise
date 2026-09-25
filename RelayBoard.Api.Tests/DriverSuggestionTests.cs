using System.Net.Http.Json;
using System.Text.Json;

namespace RelayBoard.Api.Tests;

/// <summary>
/// Draft coverage for TICKET.md. These tests are skipped until the suggestion
/// feature exists. An earlier DTO sketch used Score; the ticket's contract is
/// miles + extraMiles + slaSlipMinutes + reason. Do not add Score to make an old assertion pass.
/// </summary>
public class DriverSuggestionTests : IClassFixture<RelayBoardApiFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private readonly HttpClient _client;

    public DriverSuggestionTests(RelayBoardApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Idle_van_ranks_ahead_of_nearby_on_job_van_with_tight_sla()
    {
        var orders = await _client.GetFromJsonAsync<List<JsonElement>>("/api/orders?status=OPEN", JsonOptions);
        var vanOrder = orders!.First(o =>
            o.GetProperty("orderNumber").GetString() == "RB-1001");

        var orderId = vanOrder.GetProperty("id").GetInt32();
        var suggestions = await _client.GetFromJsonAsync<List<SuggestionSketch>>(
            $"/api/orders/{orderId}/suggestions",
            JsonOptions);

        Assert.NotNull(suggestions);
        Assert.NotEmpty(suggestions);
        Assert.All(suggestions, s => Assert.Equal("VAN", s.VehicleType));
        Assert.DoesNotContain(suggestions, s => s.DriverName == "Farid Nassar");
        Assert.Equal("Ava Chen", suggestions[0].DriverName);
        Assert.Equal(0, suggestions[0].SlaSlipMinutes);
        var carla = suggestions.FirstOrDefault(s => s.DriverName == "Carla Diaz");
        if (carla is not null)
        {
            Assert.True(carla.SlaSlipMinutes > suggestions[0].SlaSlipMinutes);
        }
    }

    [Fact]
    public async Task Non_existent_order_returns_not_found()
    {
        var response = await _client.GetAsync("/api/orders/99999/suggestions");
        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Order_without_vehicle_constraint_returns_up_to_3_suggestions()
    {
        var orders = await _client.GetFromJsonAsync<List<JsonElement>>("/api/orders?status=OPEN", JsonOptions);
        var unconstrainedOrder = orders!.First(o =>
            o.GetProperty("orderNumber").GetString() == "RB-1002");

        var orderId = unconstrainedOrder.GetProperty("id").GetInt32();
        var suggestions = await _client.GetFromJsonAsync<List<SuggestionSketch>>(
            $"/api/orders/{orderId}/suggestions",
            JsonOptions);

        Assert.NotNull(suggestions);
        Assert.True(suggestions.Count <= 3);
        Assert.NotEmpty(suggestions);
    }

    private sealed class SuggestionSketch
    {
        public int DriverId { get; set; }
        public string DriverName { get; set; } = "";
        public string VehicleType { get; set; } = "";
        public double MilesToPickup { get; set; }
        public double ExtraMiles { get; set; }
        public int SlaSlipMinutes { get; set; }
        public string Reason { get; set; } = "";
    }
}
