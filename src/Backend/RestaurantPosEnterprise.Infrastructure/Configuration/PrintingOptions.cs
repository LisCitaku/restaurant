namespace RestaurantPosEnterprise.Infrastructure.Configuration;

public class PrintingOptions
{
    public const string SectionName = "Printing";

    public string OutputDirectory { get; set; } = "PrintedReceipts";
}
