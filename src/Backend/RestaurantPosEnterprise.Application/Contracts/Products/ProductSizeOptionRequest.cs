using System.ComponentModel.DataAnnotations;

namespace RestaurantPosEnterprise.Application.Contracts.Products;

public class ProductSizeOptionRequest
{
    [Required]
    [MaxLength(20)]
    public string Name { get; set; } = string.Empty;

    [Range(0.01, 10000)]
    public decimal Price { get; set; }
}
