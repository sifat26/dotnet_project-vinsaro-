namespace Vinsaro.Application.Features.Checkout.DTOs;

public record CartDto(
    Guid Id,
    Guid UserId,
    List<CartItemDto> Items,
    decimal Subtotal
);

public record CartItemDto(
    Guid Id,
    Guid ProductVariantId,
    string ProductName,
    string? ProductImageUrl,
    string Sku,
    string? Size,
    string? Color,
    decimal UnitPrice,
    int Quantity,
    decimal TotalPrice
);

public record AddToCartRequest(
    Guid ProductVariantId,
    int Quantity
);

public record UpdateCartItemRequest(
    int Quantity
);
