using System.Security.Claims;
using RestaurantPosEnterprise.Application.Interfaces;

namespace RestaurantPosEnterprise.Api.Services;

public class UserContextService(IHttpContextAccessor httpContextAccessor) : IUserContextService
{
    public int GetRequiredUserId()
    {
        var value = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out var userId)
            ? userId
            : throw new InvalidOperationException("Authenticated user id was not found.");
    }

    public string GetRequiredUserName()
    {
        return httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Name)
            ?? throw new InvalidOperationException("Authenticated user name was not found.");
    }

    public string GetRequiredRole()
    {
        return httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Role)
            ?? throw new InvalidOperationException("Authenticated user role was not found.");
    }

    public bool IsAdmin()
    {
        var role = GetRequiredRole();
        return string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase)
            || string.Equals(role, "Manager", StringComparison.OrdinalIgnoreCase);
    }
}
