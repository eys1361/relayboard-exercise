using System.Net.Http.Json;
using System.Text.Json;

namespace RelayBoard.Api.Tests;

public class DriverApiTests : IClassFixture<RelayBoardApiFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private readonly HttpClient _client;

    public DriverApiTests(RelayBoardApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Get_drivers_includes_location_as_lat_lng()
    {
        var drivers = await _client.GetFromJsonAsync<List<DriverResponse>>("/api/drivers", JsonOptions);

        Assert.NotNull(drivers);
        Assert.Contains(drivers, d => d.Name == "Ava Chen" && d.VehicleType == "VAN" && d.Status == "AVAILABLE");
        var ava = drivers.First(d => d.Name == "Ava Chen");
        Assert.True(ava.Lat > 40);
        Assert.True(ava.Lng < -73);
        Assert.Empty(ava.CurrentPlan);
    }

    [Fact]
    public async Task On_job_driver_includes_current_plan_stops()
    {
        var drivers = await _client.GetFromJsonAsync<List<DriverResponse>>("/api/drivers", JsonOptions);
        var hector = drivers!.First(d => d.Name == "Hector Singh");
        Assert.Equal("ON_JOB", hector.Status);
        Assert.Equal(2, hector.ActiveAssignmentCount);
        Assert.Contains(hector.CurrentPlan, s => s.Kind == "DROPOFF" && s.OrderNumber == "RB-1007");
        Assert.Contains(hector.CurrentPlan, s => s.Kind == "PICKUP" && s.OrderNumber == "RB-1009");
    }

    [Fact]
    public async Task Nearby_times_square_includes_on_job_and_off_duty_drivers()
    {
        var nearby = await _client.GetFromJsonAsync<List<DriverResponse>>(
            "/api/drivers/nearby?lat=40.7580&lng=-73.9855",
            JsonOptions);

        Assert.NotNull(nearby);
        Assert.Contains(nearby, d => d.Name == "Carla Diaz" && d.Status == "ON_JOB");
        Assert.Contains(nearby, d => d.Name == "Farid Nassar" && d.Status == "OFF_DUTY");
        Assert.DoesNotContain(nearby, d => d.Name == "Devin Park");
    }

    private sealed class DriverResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string VehicleType { get; set; } = "";
        public string Status { get; set; } = "";
        public double Lat { get; set; }
        public double Lng { get; set; }
        public int ActiveAssignmentCount { get; set; }
        public List<PlanStopResponse> CurrentPlan { get; set; } = [];
    }

    private sealed class PlanStopResponse
    {
        public string Kind { get; set; } = "";
        public string OrderNumber { get; set; } = "";
    }
}
