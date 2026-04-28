using RestaurantPosEnterprise.Domain.Entities;

namespace RestaurantPosEnterprise.Infrastructure.Security;

public interface IJwtTokenGenerator
{
    (string token, DateTime expiresAtUtc) GenerateToken(AppUser user);
}
