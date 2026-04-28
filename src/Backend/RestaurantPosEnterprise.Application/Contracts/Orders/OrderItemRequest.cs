using System.ComponentModel.DataAnnotations;

namespace RestaurantPosEnterprise.Application.Contracts.Orders;

public class OrderItemRequest
{
    [Range(1, int.MaxValue)]
    public int ProductId { get; set; }

    [Range(1, 50)]
    public int Quantity { get; set; }

    [MaxLength(20)]
    public string SizeName { get; set; } = string.Empty;
}
