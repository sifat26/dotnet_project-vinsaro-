using Vinsaro.Domain.Common;
using Vinsaro.Domain.Enums;

namespace Vinsaro.Domain.Entities;

/// <summary>
/// Discount coupon. Validation (expiry, minimum order, caps, usage limits) is enforced server-side.
/// The client never supplies a discount amount — the server always calculates it.
/// </summary>
public class Coupon : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public DiscountType DiscountType { get; set; }

    /// <summary>Percentage (0–100) or fixed BDT amount depending on DiscountType.</summary>
    public decimal DiscountValue { get; set; }

    /// <summary>Minimum order subtotal (BDT) required to apply this coupon. Null = no minimum.</summary>
    public decimal? MinimumOrderAmount { get; set; }

    /// <summary>Maximum BDT discount awarded for percentage coupons. Null = no cap.</summary>
    public decimal? MaximumDiscountAmount { get; set; }

    /// <summary>Total allowed redemptions. Null = unlimited.</summary>
    public int? UsageLimit { get; set; }

    public int UsageCount { get; set; } = 0;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Order> Orders { get; set; } = [];
}
