using System.ComponentModel.DataAnnotations;

namespace RestaurantPosEnterprise.Application.Contracts.Tables;

public class UpsertTableRequest
{
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [Range(1, 20)]
    public int Seats { get; set; } = 4;
}
