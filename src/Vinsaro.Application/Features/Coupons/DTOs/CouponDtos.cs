using Vinsaro.Domain.Enums;

namespace Vinsaro.Application.Features.Coupons.DTOs;

public record CouponDto(
    Guid Id,
    string Code,
    DiscountType DiscountType,
    decimal DiscountValue,
    decimal? MinimumOrderAmount,
    decimal? MaximumDiscountAmount,
    int? UsageLimit,
    int UsageCount,
    DateTime? StartDate,
    DateTime? EndDate,
    bool IsActive
);

public record CreateCouponRequest(
    string Code,
    DiscountType DiscountType,
    decimal DiscountValue,
    decimal? MinimumOrderAmount,
    decimal? MaximumDiscountAmount,
    int? UsageLimit,
    DateTime? StartDate,
    DateTime? EndDate,
    bool IsActive
);

public record ValidateCouponResponse(
    bool IsValid,
    string? ErrorMessage,
    decimal DiscountAmount
);
