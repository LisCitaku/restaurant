using Microsoft.EntityFrameworkCore;
using RestaurantPosEnterprise.Domain.Entities;
using RestaurantPosEnterprise.Infrastructure.Data;

namespace RestaurantPosEnterprise.Infrastructure.Repositories;

public class ProductRepository(RestaurantDbContext dbContext)
{
    public async Task<List<Product>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Products
            .OrderBy(x => x.Category)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Product>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Products
            .Where(x => x.IsActive)
            .OrderBy(x => x.Category)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Product>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default)
    {
        return await dbContext.Products
            .Where(x => ids.Contains(x.Id) && x.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Products.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task AddAsync(Product product, CancellationToken cancellationToken = default)
    {
        await dbContext.Products.AddAsync(product, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
