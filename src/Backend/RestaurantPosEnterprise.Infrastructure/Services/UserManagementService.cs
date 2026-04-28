using RestaurantPosEnterprise.Application.Contracts.Users;
using RestaurantPosEnterprise.Application.Interfaces;
using RestaurantPosEnterprise.Domain.Entities;
using RestaurantPosEnterprise.Domain.Enums;
using RestaurantPosEnterprise.Infrastructure.Repositories;
using RestaurantPosEnterprise.Infrastructure.Security;

namespace RestaurantPosEnterprise.Infrastructure.Services;

public class UserManagementService(
    UserRepository userRepository,
    IPasswordHasherAdapter passwordHasher) : IUserManagementService
{
    public async Task<IReadOnlyList<UserDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var users = await userRepository.GetAllAsync(cancellationToken);
        return users.Select(Map).ToList();
    }

    public async Task<IReadOnlyList<UserDto>> GetWaitersAsync(CancellationToken cancellationToken = default)
    {
        var waiters = await userRepository.GetWaitersAsync(cancellationToken);
        return waiters.Select(Map).ToList();
    }

    public async Task<UserDto> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<UserRole>(request.Role, true, out var role))
        {
            throw new InvalidOperationException("Invalid role supplied.");
        }

        var existing = await userRepository.GetByUsernameAsync(request.Username.Trim(), cancellationToken);
        if (existing is not null)
        {
            throw new InvalidOperationException("Username is already in use.");
        }

        var user = new AppUser
        {
            Name = request.Name.Trim(),
            Username = request.Username.Trim(),
            PasswordHash = passwordHasher.Hash(request.Password),
            Role = role,
            IsActive = true
        };

        await userRepository.AddAsync(user, cancellationToken);
        await userRepository.SaveChangesAsync(cancellationToken);
        return Map(user);
    }

    public async Task<UserDto?> UpdateStatusAsync(int id, bool isActive, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdAsync(id, cancellationToken);
        if (user is null)
        {
            return null;
        }

        user.IsActive = isActive;
        await userRepository.SaveChangesAsync(cancellationToken);
        return Map(user);
    }

    private static UserDto Map(AppUser user)
    {
        return new UserDto
        {
            Id = user.Id,
            Name = user.Name,
            Username = user.Username,
            Role = user.Role.ToString(),
            IsActive = user.IsActive
        };
    }
}
