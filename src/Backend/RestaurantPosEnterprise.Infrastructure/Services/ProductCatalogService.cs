using RestaurantPosEnterprise.Application.Contracts.Products;
using RestaurantPosEnterprise.Application.Interfaces;
using RestaurantPosEnterprise.Domain.Entities;
using RestaurantPosEnterprise.Infrastructure.Repositories;
using System.Text.Json;

namespace RestaurantPosEnterprise.Infrastructure.Services;

public class ProductCatalogService(ProductRepository productRepository) : IProductCatalogService
{
    public async Task<IReadOnlyList<ProductDto>> GetActiveProductsAsync(CancellationToken cancellationToken = default)
    {
        var products = await productRepository.GetActiveAsync(cancellationToken);
        return products.Select(Map).ToList();
    }

    public async Task<IReadOnlyList<ProductDto>> GetAllProductsAsync(CancellationToken cancellationToken = default)
    {
        var products = await productRepository.GetAllAsync(cancellationToken);
        return products.Select(Map).ToList();
    }

    public async Task<ProductDto> CreateAsync(UpsertProductRequest request, CancellationToken cancellationToken = default)
    {
        var product = new Product
        {
            Name = request.Name.Trim(),
            Category = request.Category.Trim(),
            Description = request.Description.Trim(),
            ImageUrl = request.ImageUrl.Trim(),
            Price = request.Price,
            SizeOptionsJson = SerializeSizeOptions(request.SizeOptions),
            IsActive = request.IsActive
        };

        await productRepository.AddAsync(product, cancellationToken);
        await productRepository.SaveChangesAsync(cancellationToken);
        return Map(product);
    }

    public async Task<ProductDto?> UpdateAsync(int id, UpsertProductRequest request, CancellationToken cancellationToken = default)
    {
        var product = await productRepository.GetByIdAsync(id, cancellationToken);
        if (product is null)
        {
            return null;
        }

        product.Name = request.Name.Trim();
        product.Category = request.Category.Trim();
        product.Description = request.Description.Trim();
        product.ImageUrl = request.ImageUrl.Trim();
        product.Price = request.Price;
        product.SizeOptionsJson = SerializeSizeOptions(request.SizeOptions);
        product.IsActive = request.IsActive;

        await productRepository.SaveChangesAsync(cancellationToken);
        return Map(product);
    }

    private static ProductDto Map(Product product)
    {
        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Category = product.Category,
            Description = product.Description,
            ImageUrl = product.ImageUrl,
            Price = product.Price,
            SizeOptions = DeserializeSizeOptions(product.SizeOptionsJson),
            IsActive = product.IsActive
        };
    }

    private static string SerializeSizeOptions(IEnumerable<ProductSizeOptionRequest> sizeOptions)
    {
        var normalized = sizeOptions
            .Where(x => !string.IsNullOrWhiteSpace(x.Name) && x.Price > 0)
            .Select(x => new ProductSizeOptionDto
            {
                Name = x.Name.Trim().ToUpperInvariant(),
                Price = x.Price
            })
            .DistinctBy(x => x.Name)
            .OrderBy(x => x.Price)
            .ToList();

        return JsonSerializer.Serialize(normalized);
    }

    public static List<ProductSizeOptionDto> DeserializeSizeOptions(string? sizeOptionsJson)
    {
        if (string.IsNullOrWhiteSpace(sizeOptionsJson))
        {
            return new List<ProductSizeOptionDto>();
        }

        try
        {
            return JsonSerializer.Deserialize<List<ProductSizeOptionDto>>(sizeOptionsJson) ?? new List<ProductSizeOptionDto>();
        }
        catch
        {
            return new List<ProductSizeOptionDto>();
        }
    }
}
