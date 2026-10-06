using Vinsaro.Application.Common;
using Vinsaro.Application.Features.Payments.DTOs;

namespace Vinsaro.Application.Features.Payments.Interfaces;

public interface IPaymentService
{
    Task<Result<PaymentDto>> ProcessPaymentAsync(Guid userId, ProcessPaymentRequest request, CancellationToken cancellationToken = default);
    Task<Result<PaymentDto>> UpdatePaymentStatusAsync(Guid paymentId, UpdatePaymentStatusRequest request, CancellationToken cancellationToken = default);
    Task<Result<List<PaymentDto>>> GetOrderPaymentsAsync(Guid orderId, CancellationToken cancellationToken = default);
}
