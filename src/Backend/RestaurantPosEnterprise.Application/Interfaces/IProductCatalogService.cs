using RestaurantPosEnterprise.Application.Contracts.Products;

namespace RestaurantPosEnterprise.Application.Interfaces;

public interface IProductCatalogService
{
    Task<IReadOnlyList<ProductDto>> GetActiveProductsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProductDto>> GetAllProductsAsync(CancellationToken cancellationToken = default);
    Task<ProductDto> CreateAsync(UpsertProductRequest request, CancellationToken cancellationToken = default);
    Task<ProductDto?> UpdateAsync(int id, UpsertProductRequest request, CancellationToken cancellationToken = default);
}
