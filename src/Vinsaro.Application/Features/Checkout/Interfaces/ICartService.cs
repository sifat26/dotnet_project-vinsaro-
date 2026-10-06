using Vinsaro.Application.Common;
using Vinsaro.Application.Features.Checkout.DTOs;

namespace Vinsaro.Application.Features.Checkout.Interfaces;

public interface ICartService
{
    Task<Result<CartDto>> GetCartAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Result<CartDto>> AddToCartAsync(Guid userId, AddToCartRequest request, CancellationToken cancellationToken = default);
    Task<Result<CartDto>> UpdateCartItemAsync(Guid userId, Guid cartItemId, UpdateCartItemRequest request, CancellationToken cancellationToken = default);
    Task<Result<CartDto>> RemoveFromCartAsync(Guid userId, Guid cartItemId, CancellationToken cancellationToken = default);
    Task<Result<bool>> ClearCartAsync(Guid userId, CancellationToken cancellationToken = default);
}
