using Microsoft.AspNetCore.Mvc;
using RelayBoard.Api.Dtos;
using RelayBoard.Api.Services;

namespace RelayBoard.Api.Controllers;

[ApiController]
[Route("api/drivers")]
public class DriversController(IDriverService drivers) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<DriverDto>>> Get(CancellationToken cancellationToken)
    {
        return Ok(await drivers.GetDriversAsync(cancellationToken));
    }

    [HttpGet("nearby")]
    public async Task<ActionResult<IReadOnlyList<DriverDto>>> Nearby(
        [FromQuery] double lat,
        [FromQuery] double lng,
        [FromQuery] double degreeBox = 0.05,
        CancellationToken cancellationToken = default)
    {
        return Ok(await drivers.GetNearbyDriversAsync(lat, lng, degreeBox, cancellationToken));
    }
}
