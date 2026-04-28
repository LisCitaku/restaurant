using System.ComponentModel.DataAnnotations;

namespace RestaurantPosEnterprise.Application.Contracts.Users;

public class CreateUserRequest
{
    [Required]
    [MaxLength(120)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(60)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = "Waiter";
}
