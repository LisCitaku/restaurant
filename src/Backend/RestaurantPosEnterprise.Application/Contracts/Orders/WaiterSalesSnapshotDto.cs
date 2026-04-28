namespace RestaurantPosEnterprise.Application.Contracts.Orders;

public class WaiterSalesSnapshotDto
{
    public int UserId { get; set; }
    public string WaiterName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public int OrdersCount { get; set; }
    public decimal Revenue { get; set; }
}
