using RestaurantPosEnterprise.Application.Contracts.Users;

namespace RestaurantPosEnterprise.Application.Interfaces;

public interface IUserManagementService
{
    Task<IReadOnlyList<UserDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserDto>> GetWaitersAsync(CancellationToken cancellationToken = default);
    Task<UserDto> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default);
    Task<UserDto?> UpdateStatusAsync(int id, bool isActive, CancellationToken cancellationToken = default);
}
