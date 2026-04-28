using RestaurantPosEnterprise.Application.Contracts.Tables;
using RestaurantPosEnterprise.Application.Interfaces;
using RestaurantPosEnterprise.Domain.Entities;
using RestaurantPosEnterprise.Infrastructure.Repositories;

namespace RestaurantPosEnterprise.Infrastructure.Services;

public class TableService(
    TableRepository tableRepository,
    UserRepository userRepository) : ITableService
{
    public async Task<IReadOnlyList<TableDto>> GetAccessibleAsync(int? currentUserId, bool isAdmin, CancellationToken cancellationToken = default)
    {
        var tables = isAdmin || currentUserId is null
            ? await tableRepository.GetAllAsync(cancellationToken)
            : await tableRepository.GetForWaiterAsync(currentUserId.Value, cancellationToken);

        return tables.Select(Map).ToList();
    }

    public async Task<TableDto> CreateAsync(UpsertTableRequest request, CancellationToken cancellationToken = default)
    {
        var table = new DiningTable
        {
            Name = request.Name.Trim(),
            Seats = request.Seats
        };

        await tableRepository.AddAsync(table, cancellationToken);
        await tableRepository.SaveChangesAsync(cancellationToken);
        return Map(table);
    }

    public async Task<TableDto?> AssignWaiterAsync(int tableId, int? waiterId, CancellationToken cancellationToken = default)
    {
        var table = await tableRepository.GetByIdAsync(tableId, cancellationToken);
        if (table is null)
        {
            return null;
        }

        if (waiterId.HasValue)
        {
            var waiter = await userRepository.GetByIdAsync(waiterId.Value, cancellationToken);
            if (waiter is null || waiter.Role != Domain.Enums.UserRole.Waiter || !waiter.IsActive)
            {
                throw new InvalidOperationException("Selected waiter is invalid or inactive.");
            }

            table.AssignedWaiterId = waiter.Id;
            table.AssignedWaiter = waiter;
        }
        else
        {
            table.AssignedWaiterId = null;
            table.AssignedWaiter = null;
        }

        await tableRepository.SaveChangesAsync(cancellationToken);
        return Map(table);
    }

    private static TableDto Map(DiningTable table)
    {
        return new TableDto
        {
            Id = table.Id,
            Name = table.Name,
            Seats = table.Seats,
            Status = table.Status.ToString(),
            AssignedWaiterId = table.AssignedWaiterId,
            AssignedWaiterName = table.AssignedWaiter?.Name ?? "Unassigned"
        };
    }
}
