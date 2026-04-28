using RestaurantPosEnterprise.Application.Contracts.Orders;

namespace RestaurantPosEnterprise.Application.Interfaces;

public interface IKitchenTicketService
{
    Task<string> PrintNewOrderTicketAsync(OrderDto order, CancellationToken cancellationToken = default);
}
