using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantPosEnterprise.Application.Contracts.Tables;
using RestaurantPosEnterprise.Application.Interfaces;

namespace RestaurantPosEnterprise.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TablesController(
    ITableService tableService,
    IUserContextService userContextService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAccessible(CancellationToken cancellationToken)
    {
        var tables = await tableService.GetAccessibleAsync(
            userContextService.GetRequiredUserId(),
            userContextService.IsAdmin(),
            cancellationToken);

        return Ok(tables);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Create([FromBody] UpsertTableRequest request, CancellationToken cancellationToken)
    {
        var created = await tableService.CreateAsync(request, cancellationToken);
        return Ok(created);
    }

    [HttpPatch("{id:int}/assign")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> AssignWaiter(int id, [FromBody] AssignWaiterRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var updated = await tableService.AssignWaiterAsync(id, request.WaiterId, cancellationToken);
            return updated is null ? NotFound() : Ok(updated);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }
}
