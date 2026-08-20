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

    [Fact(Skip = "Implement TICKET.md")]
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
        // Assert.True(suggestions[0].Score >= suggestions[^1].Score);
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
        // public double Score { get; set; }
    }
}
