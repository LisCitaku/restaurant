using Microsoft.EntityFrameworkCore;
using RestaurantPosEnterprise.Domain.Entities;
using RestaurantPosEnterprise.Domain.Enums;
using RestaurantPosEnterprise.Infrastructure.Data;

namespace RestaurantPosEnterprise.Infrastructure.Repositories;

public class OrderRepository(RestaurantDbContext dbContext)
{
    public async Task AddAsync(Order order, CancellationToken cancellationToken = default)
    {
        await dbContext.Orders.AddAsync(order, cancellationToken);
    }

    public async Task<Order?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Orders
            .Include(x => x.Table)
            .Include(x => x.CreatedByUser)
            .Include(x => x.Items)
            .ThenInclude(x => x.Product)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<List<Order>> GetOpenOrdersAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Orders
            .Include(x => x.Table)
            .Include(x => x.CreatedByUser)
            .Include(x => x.Items)
            .Where(x => x.Status != OrderStatus.Paid && x.Status != OrderStatus.Cancelled)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Order>> GetSalesOrdersAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Orders
            .Include(x => x.CreatedByUser)
            .Include(x => x.Items)
            .Where(x => x.Status != OrderStatus.Cancelled)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
