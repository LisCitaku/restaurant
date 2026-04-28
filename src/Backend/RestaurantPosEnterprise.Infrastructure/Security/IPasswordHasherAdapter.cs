namespace RestaurantPosEnterprise.Infrastructure.Security;

public interface IPasswordHasherAdapter
{
    string Hash(string password);
    bool Verify(string hashedPassword, string providedPassword);
}
