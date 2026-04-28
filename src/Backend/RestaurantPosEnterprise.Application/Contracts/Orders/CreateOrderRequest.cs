using System.ComponentModel.DataAnnotations;

namespace RestaurantPosEnterprise.Application.Contracts.Orders;

public class CreateOrderRequest
{
    [Range(1, int.MaxValue)]
    public int TableId { get; set; }

    [MinLength(1)]
    public List<OrderItemRequest> Items { get; set; } = new();

    [MaxLength(500)]
    public string Notes { get; set; } = string.Empty;
}
