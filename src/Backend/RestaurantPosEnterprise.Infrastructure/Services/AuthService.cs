using RestaurantPosEnterprise.Application.Contracts.Auth;
using RestaurantPosEnterprise.Application.Interfaces;
using RestaurantPosEnterprise.Infrastructure.Repositories;
using RestaurantPosEnterprise.Infrastructure.Security;

namespace RestaurantPosEnterprise.Infrastructure.Services;

public class AuthService(
    UserRepository userRepository,
    IPasswordHasherAdapter passwordHasher,
    IJwtTokenGenerator jwtTokenGenerator) : IAuthService
{
    public async Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByUsernameAsync(request.Username.Trim(), cancellationToken);
        if (user is null || !user.IsActive || !passwordHasher.Verify(user.PasswordHash, request.Password))
        {
            return null;
        }

        var (token, expiresAtUtc) = jwtTokenGenerator.GenerateToken(user);
        return new LoginResponse
        {
            Token = token,
            ExpiresAtUtc = expiresAtUtc,
            Username = user.Username,
            Name = user.Name,
            Role = user.Role.ToString()
        };
    }
}
