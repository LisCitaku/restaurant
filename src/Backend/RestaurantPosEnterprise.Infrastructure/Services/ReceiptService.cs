using System.Text;
using Microsoft.Extensions.Options;
using RestaurantPosEnterprise.Application.Contracts.Receipts;
using RestaurantPosEnterprise.Application.Interfaces;
using RestaurantPosEnterprise.Domain.Enums;
using RestaurantPosEnterprise.Infrastructure.Configuration;
using RestaurantPosEnterprise.Infrastructure.Repositories;

namespace RestaurantPosEnterprise.Infrastructure.Services;

public class ReceiptService(
    OrderRepository orderRepository,
    IOptions<PrintingOptions> printingOptions) : IReceiptService
{
    private readonly PrintingOptions _printingOptions = printingOptions.Value;

    public async Task<ReceiptDto?> GenerateReceiptAsync(int orderId, string cashierName, CancellationToken cancellationToken = default)
    {
        var order = await orderRepository.GetByIdAsync(orderId, cancellationToken);
        if (order is null || order.Status != OrderStatus.Paid)
        {
            return null;
        }

        Directory.CreateDirectory(_printingOptions.OutputDirectory);
        var issuedAtUtc = DateTime.UtcNow;
        var total = order.Items.Sum(x => x.UnitPrice * x.Quantity);

        var builder = new StringBuilder();
        builder.AppendLine("RESTAURANT POS ENTERPRISE");
        builder.AppendLine("=========================");
        builder.AppendLine($"Order: #{order.Id}");
        builder.AppendLine($"Table: {order.Table?.Name ?? order.TableId.ToString()}");
        builder.AppendLine($"Cashier: {cashierName}");
        builder.AppendLine($"Issued (UTC): {issuedAtUtc:yyyy-MM-dd HH:mm:ss}");
        builder.AppendLine("-------------------------");

        foreach (var item in order.Items)
        {
            builder.AppendLine($"{item.ProductNameSnapshot} x{item.Quantity} @ {item.UnitPrice:0.00} = {(item.Quantity * item.UnitPrice):0.00}");
        }

        builder.AppendLine("-------------------------");
        builder.AppendLine($"TOTAL: {total:0.00}");

        var filePath = Path.Combine(_printingOptions.OutputDirectory, $"receipt-order-{order.Id}-{issuedAtUtc:yyyyMMddHHmmss}.txt");
        await File.WriteAllTextAsync(filePath, builder.ToString(), cancellationToken);

        return new ReceiptDto
        {
            OrderId = order.Id,
            TableName = order.Table?.Name ?? $"Table {order.TableId}",
            IssuedAtUtc = issuedAtUtc,
            CashierName = cashierName,
            Total = total,
            FilePath = Path.GetFullPath(filePath),
            Content = builder.ToString(),
            Items = order.Items.Select(x => new ReceiptItemDto
            {
                ProductName = x.ProductNameSnapshot,
                Quantity = x.Quantity,
                UnitPrice = x.UnitPrice,
                LineTotal = x.UnitPrice * x.Quantity
            }).ToList()
        };
    }
}
