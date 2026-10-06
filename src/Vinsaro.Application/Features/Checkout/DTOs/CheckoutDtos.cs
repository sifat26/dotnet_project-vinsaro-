namespace Vinsaro.Application.Features.Checkout.DTOs;

public record CheckoutRequest(
    Guid ShippingAddressId,
    string? CustomerNotes,
    string? CouponCode,
    string? IdempotencyKey
);

public record CheckoutResponse(
    Guid OrderId,
    string OrderNumber,
    decimal TotalAmount
);
