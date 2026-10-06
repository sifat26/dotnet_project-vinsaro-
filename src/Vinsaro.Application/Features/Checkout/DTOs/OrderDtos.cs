using Vinsaro.Domain.Enums;

namespace Vinsaro.Application.Features.Checkout.DTOs;

public record OrderDto(
    Guid Id,
    string OrderNumber,
    Guid UserId,
    OrderStatus OrderStatus,
    PaymentStatus PaymentStatus,
    decimal Subtotal,
    decimal DiscountAmount,
    decimal DeliveryCharge,
    decimal TotalAmount,
    string Currency,
    string CustomerName,
    string CustomerPhone,
    string ShippingAddressLine1,
    string? ShippingAddressLine2,
    string ShippingArea,
    string ShippingDistrict,
    string ShippingDivision,
    string? ShippingPostalCode,
    string? CustomerNotes,
    DateTime CreatedAt,
    List<OrderItemDto> Items
);

public record OrderItemDto(
    Guid Id,
    Guid? ProductVariantId,
    string ProductName,
    string Sku,
    string? SelectedColor,
    string? SelectedSize,
    decimal UnitPrice,
    int Quantity,
    decimal TotalPrice
);

public record OrderSearchFilter(
    Guid? UserId,
    OrderStatus? OrderStatus,
    PaymentStatus? PaymentStatus,
    DateTime? StartDate,
    DateTime? EndDate,
    int Page = 1,
    int PageSize = 20
);
