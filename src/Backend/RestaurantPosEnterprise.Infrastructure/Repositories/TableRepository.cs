using Microsoft.EntityFrameworkCore;
using RestaurantPosEnterprise.Domain.Entities;
using RestaurantPosEnterprise.Infrastructure.Data;

namespace RestaurantPosEnterprise.Infrastructure.Repositories;

public class TableRepository(RestaurantDbContext dbContext)
{
    public async Task<List<DiningTable>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Tables
            .Include(x => x.AssignedWaiter)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<DiningTable>> GetForWaiterAsync(int waiterId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Tables
            .Include(x => x.AssignedWaiter)
            .Where(x => x.AssignedWaiterId == waiterId)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<DiningTable?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Tables
            .Include(x => x.AssignedWaiter)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task AddAsync(DiningTable table, CancellationToken cancellationToken = default)
    {
        await dbContext.Tables.AddAsync(table, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
