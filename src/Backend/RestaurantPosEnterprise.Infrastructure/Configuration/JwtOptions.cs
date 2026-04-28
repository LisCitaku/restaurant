namespace RestaurantPosEnterprise.Infrastructure.Configuration;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "RestaurantPosEnterprise";
    public string Audience { get; set; } = "RestaurantPosEnterprise.Client";
    public string SigningKey { get; set; } = "ChangeThisSigningKeyForProduction123!";
    public int ExpirationMinutes { get; set; } = 480;
}
