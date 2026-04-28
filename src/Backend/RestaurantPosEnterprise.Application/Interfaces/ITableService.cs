using RestaurantPosEnterprise.Application.Contracts.Tables;

namespace RestaurantPosEnterprise.Application.Interfaces;

public interface ITableService
{
    Task<IReadOnlyList<TableDto>> GetAccessibleAsync(int? currentUserId, bool isAdmin, CancellationToken cancellationToken = default);
    Task<TableDto> CreateAsync(UpsertTableRequest request, CancellationToken cancellationToken = default);
    Task<TableDto?> AssignWaiterAsync(int tableId, int? waiterId, CancellationToken cancellationToken = default);
}
