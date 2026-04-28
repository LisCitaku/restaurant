using RestaurantPosEnterprise.Application.Contracts.Orders;

namespace RestaurantPosEnterprise.Application.Interfaces;

public interface IOrderService
{
    Task<OrderDto> CreateOrderAsync(CreateOrderRequest request, int createdByUserId, bool isAdmin, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OrderDto>> GetOpenOrdersAsync(int currentUserId, bool isAdmin, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OrderDto>> GetAllOpenOrdersAsync(CancellationToken cancellationToken = default);
    Task<SalesSummaryDto> GetSalesSummaryAsync(CancellationToken cancellationToken = default);
    Task<OrderDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> MarkPaidAsync(int id, CancellationToken cancellationToken = default);
}
