using Vinsaro.Domain.Enums;

namespace Vinsaro.Application.Features.Payments.DTOs;

public record PaymentDto(
    Guid Id,
    Guid OrderId,
    PaymentMethod PaymentMethod,
    PaymentStatus PaymentStatus,
    decimal Amount,
    string Currency,
    string? ProviderTransactionId,
    string? ProviderReference,
    DateTime? PaidAt
);

public record ProcessPaymentRequest(
    Guid OrderId,
    PaymentMethod PaymentMethod,
    string? ProviderTransactionId
);

public record UpdatePaymentStatusRequest(
    PaymentStatus PaymentStatus,
    string? ProviderTransactionId
);
