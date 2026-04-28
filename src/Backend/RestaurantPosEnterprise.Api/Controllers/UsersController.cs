using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantPosEnterprise.Application.Contracts.Users;
using RestaurantPosEnterprise.Application.Interfaces;

namespace RestaurantPosEnterprise.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Manager")]
public class UsersController(IUserManagementService userManagementService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var users = await userManagementService.GetAllAsync(cancellationToken);
        return Ok(users);
    }

    [HttpGet("waiters")]
    public async Task<IActionResult> GetWaiters(CancellationToken cancellationToken)
    {
        var waiters = await userManagementService.GetWaitersAsync(cancellationToken);
        return Ok(waiters);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var created = await userManagementService.CreateAsync(request, cancellationToken);
            return Ok(created);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateUserStatusRequest request, CancellationToken cancellationToken)
    {
        var updated = await userManagementService.UpdateStatusAsync(id, request.IsActive, cancellationToken);
        return updated is null ? NotFound() : Ok(updated);
    }
}
