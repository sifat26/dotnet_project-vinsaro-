using Vinsaro.Application.Common;
using Vinsaro.Application.Features.Checkout.DTOs;

namespace Vinsaro.Application.Features.Checkout.Interfaces;

public interface ICheckoutService
{
    Task<Result<CheckoutResponse>> ProcessCheckoutAsync(Guid userId, CheckoutRequest request, CancellationToken cancellationToken = default);
}
