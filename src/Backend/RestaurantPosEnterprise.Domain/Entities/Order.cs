using RestaurantPosEnterprise.Domain.Enums;

namespace RestaurantPosEnterprise.Domain.Entities;

public class Order
{
    public int Id { get; set; }
    public int TableId { get; set; }
    public int CreatedByUserId { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Open;
    public string Notes { get; set; } = string.Empty;
    public DiningTable? Table { get; set; }
    public AppUser? CreatedByUser { get; set; }
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}
