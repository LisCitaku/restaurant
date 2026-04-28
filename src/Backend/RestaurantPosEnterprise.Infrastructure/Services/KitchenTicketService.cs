using System.Text;
using Microsoft.Extensions.Options;
using RestaurantPosEnterprise.Application.Contracts.Orders;
using RestaurantPosEnterprise.Application.Interfaces;
using RestaurantPosEnterprise.Infrastructure.Configuration;

namespace RestaurantPosEnterprise.Infrastructure.Services;

public class KitchenTicketService(IOptions<KitchenTicketOptions> options) : IKitchenTicketService
{
    private readonly KitchenTicketOptions _options = options.Value;

    public async Task<string> PrintNewOrderTicketAsync(OrderDto order, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(_options.OutputDirectory);

        var builder = new StringBuilder();
        builder.AppendLine("FAVELLA KITCHEN TICKET");
        builder.AppendLine("======================");
        builder.AppendLine($"Order: #{order.Id}");
        builder.AppendLine($"Table: {order.TableName}");
        builder.AppendLine($"Waiter: {order.CreatedBy}");
        builder.AppendLine($"Created (UTC): {order.CreatedAtUtc:yyyy-MM-dd HH:mm:ss}");
        builder.AppendLine($"Notes: {order.Notes}");
        builder.AppendLine("----------------------");

        foreach (var item in order.Items)
        {
            builder.AppendLine($"{item.ProductName} x{item.Quantity}");
        }

        var path = Path.Combine(_options.OutputDirectory, $"kitchen-order-{order.Id}.txt");
        await File.WriteAllTextAsync(path, builder.ToString(), cancellationToken);
        return Path.GetFullPath(path);
    }
}
