using RestaurantPosEnterprise.Domain.Enums;

namespace RestaurantPosEnterprise.Domain.Entities;

public class AppUser
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<DiningTable> AssignedTables { get; set; } = new List<DiningTable>();
}
