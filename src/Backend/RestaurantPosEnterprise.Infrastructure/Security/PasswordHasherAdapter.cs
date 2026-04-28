using System.Security.Cryptography;
using System.Text;

namespace RestaurantPosEnterprise.Infrastructure.Security;

public class PasswordHasherAdapter : IPasswordHasherAdapter
{
    private const string Salt = "RestaurantPosEnterprise.FixedSalt";

    public string Hash(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes($"{Salt}:{password}"));
        return Convert.ToHexString(bytes);
    }

    public bool Verify(string hashedPassword, string providedPassword)
    {
        return string.Equals(hashedPassword, Hash(providedPassword), StringComparison.OrdinalIgnoreCase);
    }
}
