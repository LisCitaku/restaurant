using RestaurantPosEnterprise.Application.Contracts.Orders;
using RestaurantPosEnterprise.Application.Contracts.Products;
using RestaurantPosEnterprise.Application.Interfaces;
using RestaurantPosEnterprise.Domain.Entities;
using RestaurantPosEnterprise.Domain.Enums;
using RestaurantPosEnterprise.Infrastructure.Repositories;

namespace RestaurantPosEnterprise.Infrastructure.Services;

public class OrderService(
    OrderRepository orderRepository,
    TableRepository tableRepository,
    ProductRepository productRepository,
    UserRepository userRepository,
    IKitchenTicketService kitchenTicketService) : IOrderService
{
    public async Task<OrderDto> CreateOrderAsync(CreateOrderRequest request, int createdByUserId, bool isAdmin, CancellationToken cancellationToken = default)
    {
        var table = await tableRepository.GetByIdAsync(request.TableId, cancellationToken)
            ?? throw new InvalidOperationException("Selected table was not found.");

        if (!isAdmin && table.AssignedWaiterId.HasValue && table.AssignedWaiterId != createdByUserId)
        {
            throw new InvalidOperationException("This table is assigned to another waiter.");
        }

        var requestedIds = request.Items.Select(x => x.ProductId).Distinct().ToList();
        var products = await productRepository.GetByIdsAsync(requestedIds, cancellationToken);
        if (products.Count != requestedIds.Count)
        {
            throw new InvalidOperationException("One or more selected products are invalid.");
        }

        var order = new Order
        {
            TableId = request.TableId,
            CreatedByUserId = createdByUserId,
            CreatedAtUtc = DateTime.UtcNow,
            Status = OrderStatus.Submitted,
            Notes = request.Notes.Trim()
        };

        foreach (var item in request.Items)
        {
            var product = products.First(x => x.Id == item.ProductId);
            var sizeOptions = ProductCatalogService.DeserializeSizeOptions(product.SizeOptionsJson);
            var selectedSize = item.SizeName?.Trim().ToUpperInvariant() ?? string.Empty;
            var unitPrice = ResolveUnitPrice(product, sizeOptions, selectedSize);

            order.Items.Add(new OrderItem
            {
                ProductId = product.Id,
                Quantity = item.Quantity,
                UnitPrice = unitPrice,
                ProductNameSnapshot = BuildProductSnapshotName(product.Name, selectedSize),
                SelectedSize = selectedSize
            });
        }

        table.Status = TableStatus.Occupied;
        await orderRepository.AddAsync(order, cancellationToken);
        await orderRepository.SaveChangesAsync(cancellationToken);

        var savedOrder = await orderRepository.GetByIdAsync(order.Id, cancellationToken)
            ?? throw new InvalidOperationException("The created order could not be loaded.");

        var result = await MapAsync(savedOrder, cancellationToken);
        await kitchenTicketService.PrintNewOrderTicketAsync(result, cancellationToken);
        return result;
    }

    public async Task<IReadOnlyList<OrderDto>> GetOpenOrdersAsync(int currentUserId, bool isAdmin, CancellationToken cancellationToken = default)
    {
        var orders = await orderRepository.GetOpenOrdersAsync(cancellationToken);
        if (!isAdmin)
        {
            orders = orders
                .Where(x => x.CreatedByUserId == currentUserId || x.Table?.AssignedWaiterId == currentUserId)
                .ToList();
        }

        var result = new List<OrderDto>(orders.Count);
        foreach (var order in orders)
        {
            result.Add(await MapAsync(order, cancellationToken));
        }

        return result;
    }

    public async Task<IReadOnlyList<OrderDto>> GetAllOpenOrdersAsync(CancellationToken cancellationToken = default)
    {
        var orders = await orderRepository.GetOpenOrdersAsync(cancellationToken);
        var result = new List<OrderDto>(orders.Count);

        foreach (var order in orders)
        {
            result.Add(await MapAsync(order, cancellationToken));
        }

        return result;
    }

    public async Task<SalesSummaryDto> GetSalesSummaryAsync(CancellationToken cancellationToken = default)
    {
        var orders = await orderRepository.GetSalesOrdersAsync(cancellationToken);
        var nowUtc = DateTime.UtcNow;
        var dayStartUtc = nowUtc.AddHours(-24);
        var monthStartUtc = new DateTime(nowUtc.Year, nowUtc.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var dailyOrders = orders.Where(x => x.CreatedAtUtc >= dayStartUtc).ToList();
        var monthlyOrders = orders.Where(x => x.CreatedAtUtc >= monthStartUtc).ToList();

        return new SalesSummaryDto
        {
            GeneratedAtUtc = nowUtc,
            DailyTotal = dailyOrders.Sum(CalculateOrderTotal),
            MonthlyTotal = monthlyOrders.Sum(CalculateOrderTotal),
            DailyByWaiter = BuildWaiterSalesSnapshots(dailyOrders),
            MonthlyByWaiter = BuildWaiterSalesSnapshots(monthlyOrders)
        };
    }

    public async Task<OrderDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var order = await orderRepository.GetByIdAsync(id, cancellationToken);
        return order is null ? null : await MapAsync(order, cancellationToken);
    }

    public async Task<bool> MarkPaidAsync(int id, CancellationToken cancellationToken = default)
    {
        var order = await orderRepository.GetByIdAsync(id, cancellationToken);
        if (order is null)
        {
            return false;
        }

        order.Status = OrderStatus.Paid;
        if (order.Table is not null)
        {
            order.Table.Status = TableStatus.Available;
        }

        await orderRepository.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task<OrderDto> MapAsync(Order order, CancellationToken cancellationToken)
    {
        var creator = order.CreatedByUser ?? await userRepository.GetByIdAsync(order.CreatedByUserId, cancellationToken);

        return new OrderDto
        {
            Id = order.Id,
            TableId = order.TableId,
            TableName = order.Table?.Name ?? $"Table {order.TableId}",
            CreatedBy = creator?.Name ?? "Unknown",
            CreatedAtUtc = order.CreatedAtUtc,
            Status = order.Status.ToString(),
            Notes = order.Notes,
            Total = order.Items.Sum(x => x.UnitPrice * x.Quantity),
            Items = order.Items.Select(x => new OrderItemDto
            {
                Id = x.Id,
                ProductId = x.ProductId,
                ProductName = x.ProductNameSnapshot,
                SelectedSize = x.SelectedSize,
                Quantity = x.Quantity,
                UnitPrice = x.UnitPrice,
                LineTotal = x.UnitPrice * x.Quantity
            }).ToList()
        };
    }

    private static decimal ResolveUnitPrice(Product product, IReadOnlyList<ProductSizeOptionDto> sizeOptions, string selectedSize)
    {
        if (sizeOptions.Count == 0)
        {
            return product.Price;
        }

        if (string.IsNullOrWhiteSpace(selectedSize))
        {
            throw new InvalidOperationException($"Select a size for {product.Name} before placing the order.");
        }

        var matchedSize = sizeOptions.FirstOrDefault(x => x.Name.Equals(selectedSize, StringComparison.OrdinalIgnoreCase));
        if (matchedSize is null)
        {
            throw new InvalidOperationException($"Selected size is invalid for {product.Name}.");
        }

        return matchedSize.Price;
    }

    private static string BuildProductSnapshotName(string productName, string selectedSize)
    {
        return string.IsNullOrWhiteSpace(selectedSize)
            ? productName
            : $"{productName} ({selectedSize})";
    }

    private static decimal CalculateOrderTotal(Order order)
    {
        return order.Items.Sum(x => x.UnitPrice * x.Quantity);
    }

    private static List<WaiterSalesSnapshotDto> BuildWaiterSalesSnapshots(IEnumerable<Order> orders)
    {
        return orders
            .GroupBy(order => new
            {
                UserId = order.CreatedByUserId,
                WaiterName = order.CreatedByUser?.Name ?? "Unknown",
                Username = order.CreatedByUser?.Username ?? "unknown"
            })
            .Select(group => new WaiterSalesSnapshotDto
            {
                UserId = group.Key.UserId,
                WaiterName = group.Key.WaiterName,
                Username = group.Key.Username,
                OrdersCount = group.Count(),
                Revenue = group.Sum(CalculateOrderTotal)
            })
            .OrderByDescending(x => x.Revenue)
            .ThenBy(x => x.WaiterName)
            .ToList();
    }
}
