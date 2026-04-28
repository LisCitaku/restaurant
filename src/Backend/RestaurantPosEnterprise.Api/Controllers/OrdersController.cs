using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantPosEnterprise.Application.Contracts.Orders;
using RestaurantPosEnterprise.Application.Interfaces;

namespace RestaurantPosEnterprise.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController(
    IOrderService orderService,
    IUserContextService userContextService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetOpenOrders(CancellationToken cancellationToken)
    {
        var orders = await orderService.GetOpenOrdersAsync(
            userContextService.GetRequiredUserId(),
            userContextService.IsAdmin(),
            cancellationToken);

        return Ok(orders);
    }

    [HttpGet("admin-feed")]
    [Authorize(Roles = "Admin,Manager,Cashier")]
    public async Task<IActionResult> GetAdminFeed(CancellationToken cancellationToken)
    {
        var orders = await orderService.GetAllOpenOrdersAsync(cancellationToken);
        return Ok(orders);
    }

    [HttpGet("analytics/summary")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetSalesSummary(CancellationToken cancellationToken)
    {
        var summary = await orderService.GetSalesSummaryAsync(cancellationToken);
        return Ok(summary);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var order = await orderService.GetByIdAsync(id, cancellationToken);
        return order is null ? NotFound() : Ok(order);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager,Waiter")]
    public async Task<IActionResult> Create([FromBody] CreateOrderRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var created = await orderService.CreateOrderAsync(
                request,
                userContextService.GetRequiredUserId(),
                userContextService.IsAdmin(),
                cancellationToken);

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpPost("{id:int}/pay")]
    [Authorize(Roles = "Admin,Manager,Cashier")]
    public async Task<IActionResult> MarkPaid(int id, CancellationToken cancellationToken)
    {
        var updated = await orderService.MarkPaidAsync(id, cancellationToken);
        return updated ? Ok(new { message = $"Order {id} marked as paid." }) : NotFound();
    }
}
