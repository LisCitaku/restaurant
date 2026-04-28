using System.ComponentModel.DataAnnotations;

namespace RestaurantPosEnterprise.Application.Contracts.Products;

public class UpsertProductRequest
{
    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(60)]
    public string Category { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(500)]
    public string ImageUrl { get; set; } = string.Empty;

    [Range(0.01, 10000)]
    public decimal Price { get; set; }

    public List<ProductSizeOptionRequest> SizeOptions { get; set; } = new();

    public bool IsActive { get; set; } = true;
}
