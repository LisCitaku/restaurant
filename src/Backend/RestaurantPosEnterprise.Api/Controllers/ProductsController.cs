using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantPosEnterprise.Application.Contracts.Products;
using RestaurantPosEnterprise.Application.Interfaces;

namespace RestaurantPosEnterprise.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductsController(IProductCatalogService productCatalogService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetActive(CancellationToken cancellationToken)
    {
        var products = await productCatalogService.GetActiveProductsAsync(cancellationToken);
        return Ok(products);
    }

    [HttpGet("all")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var products = await productCatalogService.GetAllProductsAsync(cancellationToken);
        return Ok(products);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Create([FromBody] UpsertProductRequest request, CancellationToken cancellationToken)
    {
        var created = await productCatalogService.CreateAsync(request, cancellationToken);
        return Ok(created);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Update(int id, [FromBody] UpsertProductRequest request, CancellationToken cancellationToken)
    {
        var updated = await productCatalogService.UpdateAsync(id, request, cancellationToken);
        return updated is null ? NotFound() : Ok(updated);
    }
}
