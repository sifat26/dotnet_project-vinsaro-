using Vinsaro.Application.Common;
using Vinsaro.Application.Features.Checkout.DTOs;

namespace Vinsaro.Application.Features.Checkout.Interfaces;

public interface IOrderService
{
    Task<Result<OrderDto>> GetOrderByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<OrderDto>> GetOrderByNumberAsync(string orderNumber, CancellationToken cancellationToken = default);
    Task<Result<PagedResult<OrderDto>>> SearchOrdersAsync(OrderSearchFilter filter, CancellationToken cancellationToken = default);
    Task<Result<OrderDto>> UpdateOrderStatusAsync(Guid orderId, Vinsaro.Domain.Enums.OrderStatus status, CancellationToken cancellationToken = default);
}
