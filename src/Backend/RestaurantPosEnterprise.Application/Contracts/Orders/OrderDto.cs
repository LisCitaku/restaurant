namespace RestaurantPosEnterprise.Application.Contracts.Orders;

public class OrderDto
{
    public int Id { get; set; }
    public int TableId { get; set; }
    public string TableName { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
}
