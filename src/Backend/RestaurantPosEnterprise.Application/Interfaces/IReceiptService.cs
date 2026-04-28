using RestaurantPosEnterprise.Application.Contracts.Receipts;

namespace RestaurantPosEnterprise.Application.Interfaces;

public interface IReceiptService
{
    Task<ReceiptDto?> GenerateReceiptAsync(int orderId, string cashierName, CancellationToken cancellationToken = default);
}
