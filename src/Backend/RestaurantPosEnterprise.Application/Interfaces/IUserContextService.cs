namespace RestaurantPosEnterprise.Application.Interfaces;

public interface IUserContextService
{
    int GetRequiredUserId();
    string GetRequiredUserName();
    string GetRequiredRole();
    bool IsAdmin();
}
