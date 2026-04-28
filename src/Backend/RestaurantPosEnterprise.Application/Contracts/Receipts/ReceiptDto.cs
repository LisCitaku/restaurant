namespace RestaurantPosEnterprise.Application.Contracts.Receipts;

public class ReceiptDto
{
    public int OrderId { get; set; }
    public string TableName { get; set; } = string.Empty;
    public DateTime IssuedAtUtc { get; set; }
    public string CashierName { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public List<ReceiptItemDto> Items { get; set; } = new();
}
