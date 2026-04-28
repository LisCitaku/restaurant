using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using RestaurantPosEnterprise.Domain.Entities;
using RestaurantPosEnterprise.Domain.Enums;
using RestaurantPosEnterprise.Infrastructure.Security;

namespace RestaurantPosEnterprise.Infrastructure.Data;

public class RestaurantDbInitializer(RestaurantDbContext dbContext, IPasswordHasherAdapter passwordHasher)
{
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await dbContext.Database.EnsureCreatedAsync(cancellationToken);
        await ApplySchemaUpgradesAsync(cancellationToken);
        await EnsureDefaultUsersAsync(cancellationToken);

        if (!await dbContext.Tables.AnyAsync(cancellationToken))
        {
            dbContext.Tables.AddRange(
                new DiningTable { Name = "Table 1", Seats = 4, Status = TableStatus.Available },
                new DiningTable { Name = "Table 2", Seats = 2, Status = TableStatus.Available },
                new DiningTable { Name = "Table 3", Seats = 6, Status = TableStatus.Reserved },
                new DiningTable { Name = "Table 4", Seats = 4, Status = TableStatus.Available });
        }

        if (!await dbContext.Products.AnyAsync(cancellationToken))
        {
            dbContext.Products.AddRange(
                new Product
                {
                    Name = "Margherita Pizza",
                    Category = "Pizza",
                    Description = "Tomato, mozzarella and basil on a crisp hand-stretched base.",
                    Price = 9.50m,
                    SizeOptionsJson = JsonSerializer.Serialize(new[]
                    {
                        new { Name = "S", Price = 8.50m },
                        new { Name = "M", Price = 9.50m },
                        new { Name = "L", Price = 11.50m }
                    }),
                    IsActive = true
                },
                new Product { Name = "Caesar Salad", Category = "Salad", Description = "Romaine lettuce, parmesan, croutons and signature dressing.", Price = 6.80m, IsActive = true },
                new Product { Name = "Grilled Salmon", Category = "Main", Description = "Fresh salmon with herb butter, lemon and seasonal vegetables.", Price = 14.90m, IsActive = true },
                new Product { Name = "Fresh Lemonade", Category = "Drinks", Description = "House-made lemonade with mint and sparkling finish.", Price = 3.20m, IsActive = true },
                new Product { Name = "Chocolate Cake", Category = "Dessert", Description = "Rich dark chocolate sponge with soft ganache center.", Price = 4.70m, IsActive = true });
        }
        else
        {
            var margherita = await dbContext.Products.FirstOrDefaultAsync(x => x.Name == "Margherita Pizza", cancellationToken);
            if (margherita is not null && string.IsNullOrWhiteSpace(margherita.SizeOptionsJson))
            {
                margherita.SizeOptionsJson = JsonSerializer.Serialize(new[]
                {
                    new { Name = "S", Price = 8.50m },
                    new { Name = "M", Price = 9.50m },
                    new { Name = "L", Price = 11.50m }
                });
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsureDefaultUsersAsync(CancellationToken cancellationToken)
    {
        await UpsertDefaultUserAsync("admin", "System Admin", "admin123", UserRole.Admin, cancellationToken);
        await UpsertDefaultUserAsync("waiter", "Floor Waiter", "waiter123", UserRole.Waiter, cancellationToken);
        await UpsertDefaultUserAsync("cashier", "Front Cashier", "cashier123", UserRole.Cashier, cancellationToken);
    }

    private async Task UpsertDefaultUserAsync(
        string username,
        string name,
        string password,
        UserRole role,
        CancellationToken cancellationToken)
    {
        var existing = await dbContext.Users.FirstOrDefaultAsync(x => x.Username == username, cancellationToken);
        var hashedPassword = passwordHasher.Hash(password);

        if (existing is null)
        {
            await dbContext.Users.AddAsync(new AppUser
            {
                Name = name,
                Username = username,
                PasswordHash = hashedPassword,
                Role = role,
                IsActive = true
            }, cancellationToken);

            return;
        }

        existing.Name = name;
        existing.Role = role;
        existing.IsActive = true;
        existing.PasswordHash = hashedPassword;
    }

    private async Task ApplySchemaUpgradesAsync(CancellationToken cancellationToken)
    {
        await dbContext.Database.ExecuteSqlRawAsync("""
IF COL_LENGTH('Users', 'IsActive') IS NULL
BEGIN
    ALTER TABLE [Users] ADD [IsActive] bit NOT NULL CONSTRAINT DF_Users_IsActive DEFAULT(1);
END
""", cancellationToken);

        await dbContext.Database.ExecuteSqlRawAsync("""
IF COL_LENGTH('Tables', 'Seats') IS NULL
BEGIN
    ALTER TABLE [Tables] ADD [Seats] int NOT NULL CONSTRAINT DF_Tables_Seats DEFAULT(4);
END
""", cancellationToken);

        await dbContext.Database.ExecuteSqlRawAsync("""
IF COL_LENGTH('Tables', 'AssignedWaiterId') IS NULL
BEGIN
    ALTER TABLE [Tables] ADD [AssignedWaiterId] int NULL;
END
""", cancellationToken);

        await dbContext.Database.ExecuteSqlRawAsync("""
IF NOT EXISTS (
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = 'FK_Tables_Users_AssignedWaiterId'
)
BEGIN
    ALTER TABLE [Tables]
    ADD CONSTRAINT [FK_Tables_Users_AssignedWaiterId]
    FOREIGN KEY ([AssignedWaiterId]) REFERENCES [Users]([Id]) ON DELETE SET NULL;
END
""", cancellationToken);

        await dbContext.Database.ExecuteSqlRawAsync("""
IF COL_LENGTH('Products', 'Description') IS NULL
BEGIN
    ALTER TABLE [Products] ADD [Description] nvarchar(500) NOT NULL CONSTRAINT DF_Products_Description DEFAULT('');
END
""", cancellationToken);

        await dbContext.Database.ExecuteSqlRawAsync("""
IF COL_LENGTH('Products', 'ImageUrl') IS NULL
BEGIN
    ALTER TABLE [Products] ADD [ImageUrl] nvarchar(500) NOT NULL CONSTRAINT DF_Products_ImageUrl DEFAULT('');
END
""", cancellationToken);

        await dbContext.Database.ExecuteSqlRawAsync("""
IF COL_LENGTH('Products', 'SizeOptionsJson') IS NULL
BEGIN
    ALTER TABLE [Products] ADD [SizeOptionsJson] nvarchar(max) NOT NULL CONSTRAINT DF_Products_SizeOptionsJson DEFAULT('[]');
END
""", cancellationToken);

        await dbContext.Database.ExecuteSqlRawAsync("""
IF COL_LENGTH('OrderItems', 'SelectedSize') IS NULL
BEGIN
    ALTER TABLE [OrderItems] ADD [SelectedSize] nvarchar(20) NOT NULL CONSTRAINT DF_OrderItems_SelectedSize DEFAULT('');
END
""", cancellationToken);
    }
}
