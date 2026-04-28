using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantPosEnterprise.Application.Interfaces;

namespace RestaurantPosEnterprise.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Manager,Cashier")]
public class ReceiptsController(
    IReceiptService receiptService,
    IUserContextService userContextService) : ControllerBase
{
    [HttpPost("{orderId:int}")]
    public async Task<IActionResult> Generate(int orderId, CancellationToken cancellationToken)
    {
        var receipt = await receiptService.GenerateReceiptAsync(orderId, userContextService.GetRequiredUserName(), cancellationToken);
        return receipt is null
            ? BadRequest(new { message = "Receipt can be generated only for paid orders." })
            : Ok(receipt);
    }
}
