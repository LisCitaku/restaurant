using RestaurantPosEnterprise.Domain.Enums;

namespace RestaurantPosEnterprise.Domain.Entities;

public class DiningTable
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Seats { get; set; } = 4;
    public TableStatus Status { get; set; } = TableStatus.Available;
    public int? AssignedWaiterId { get; set; }
    public AppUser? AssignedWaiter { get; set; }
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
