using RestaurantPosEnterprise.Application.Contracts.Auth;

namespace RestaurantPosEnterprise.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
}
