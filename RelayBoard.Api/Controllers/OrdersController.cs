using Microsoft.AspNetCore.Mvc;
using RelayBoard.Api.Dtos;
using RelayBoard.Api.Services;

namespace RelayBoard.Api.Controllers;

[ApiController]
[Route("api/orders")]
public class OrdersController(IOrderService orders) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<OrderDto>>> Get(
        [FromQuery] string? status,
        CancellationToken cancellationToken)
    {
        return Ok(await orders.GetOrdersAsync(status, cancellationToken));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<OrderDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var order = await orders.GetOrderAsync(id, cancellationToken);
        return order is null ? NotFound() : Ok(order);
    }

    [HttpPost("{id:int}/assign")]
    public async Task<ActionResult<OrderDto>> Assign(
        int id,
        [FromBody] AssignDriverRequest request,
        CancellationToken cancellationToken)
    {
        var result = await orders.AssignAsync(id, request.DriverId, cancellationToken);
        if (!result.Found)
        {
            return NotFound();
        }

        if (result.Error is not null)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Order);
    }
}
