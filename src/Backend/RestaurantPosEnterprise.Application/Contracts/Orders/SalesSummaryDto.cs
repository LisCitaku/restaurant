namespace RestaurantPosEnterprise.Application.Contracts.Orders;

public class SalesSummaryDto
{
    public DateTime GeneratedAtUtc { get; set; }
    public decimal DailyTotal { get; set; }
    public decimal MonthlyTotal { get; set; }
    public List<WaiterSalesSnapshotDto> DailyByWaiter { get; set; } = new();
    public List<WaiterSalesSnapshotDto> MonthlyByWaiter { get; set; } = new();
}
