using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RelayBoard.Api.Data;
using RelayBoard.Api.Dtos;

namespace RelayBoard.Api.Controllers;

[ApiController]
[Route("api/lookups")]
public class LookupsController(RelayBoardContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<LookupsDto>> Get(CancellationToken cancellationToken)
    {
        return Ok(new LookupsDto
        {
            VehicleTypes = await db.VehicleTypes.AsNoTracking()
                .OrderBy(v => v.Id)
                .Select(v => new LookupDto { Id = v.Id, Code = v.Code, Name = v.Name })
                .ToListAsync(cancellationToken),
            DriverStatuses = await db.DriverStatuses.AsNoTracking()
                .OrderBy(v => v.Id)
                .Select(v => new LookupDto { Id = v.Id, Code = v.Code, Name = v.Name })
                .ToListAsync(cancellationToken),
            OrderStatuses = await db.OrderStatuses.AsNoTracking()
                .OrderBy(v => v.Id)
                .Select(v => new LookupDto { Id = v.Id, Code = v.Code, Name = v.Name })
                .ToListAsync(cancellationToken),
        });
    }
}
