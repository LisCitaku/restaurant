using Microsoft.EntityFrameworkCore;
using RestaurantPosEnterprise.Domain.Entities;
using RestaurantPosEnterprise.Infrastructure.Data;

namespace RestaurantPosEnterprise.Infrastructure.Repositories;

public class UserRepository(RestaurantDbContext dbContext)
{
    public async Task<List<AppUser>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Users
            .OrderBy(x => x.Role)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<AppUser>> GetWaitersAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Users
            .Where(x => x.Role == Domain.Enums.UserRole.Waiter && x.IsActive)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<AppUser?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        return await dbContext.Users.FirstOrDefaultAsync(x => x.Username == username, cancellationToken);
    }

    public async Task<AppUser?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Users.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task AddAsync(AppUser user, CancellationToken cancellationToken = default)
    {
        await dbContext.Users.AddAsync(user, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
