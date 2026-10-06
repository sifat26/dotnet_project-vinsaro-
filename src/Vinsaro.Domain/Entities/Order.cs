using Vinsaro.Domain.Common;
using Vinsaro.Domain.Enums;

namespace Vinsaro.Domain.Entities;

/// <summary>
/// Customer order. Snapshots of customer/shipping data are stored at order time
/// so historical orders remain accurate even if the customer updates their profile.
/// Currency defaults to BDT for Bangladeshi market.
/// </summary>
public class Order : BaseEntity
{
    public string OrderNumber { get; set; } = string.Empty;

    public Guid UserId { get; set; }

    public OrderStatus OrderStatus { get; set; } = OrderStatus.Pending;
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Unpaid;

    // Financials — all in BDT, stored as NUMERIC(18,2)
    public decimal Subtotal { get; set; }
    public decimal DiscountAmount { get; set; } = 0;
    public decimal DeliveryCharge { get; set; } = 0;
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "BDT";

    // Customer snapshot at order time (immutable historical record)
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;

    // Shipping address snapshot at order time
    public string ShippingAddressLine1 { get; set; } = string.Empty;
    public string? ShippingAddressLine2 { get; set; }
    public string ShippingArea { get; set; } = string.Empty;
    public string ShippingDistrict { get; set; } = string.Empty;
    public string ShippingDivision { get; set; } = string.Empty;
    public string? ShippingPostalCode { get; set; }

    public string? CustomerNotes { get; set; }

    // Applied coupon (if any)
    public Guid? CouponId { get; set; }
    public Coupon? Coupon { get; set; }

    // Idempotency: optional client-supplied key to detect duplicate checkout requests
    public string? IdempotencyKey { get; set; }

    // Track if cancellation was already processed — prevents double-restocking
    public bool InventoryRestored { get; set; } = false;

    public ICollection<OrderItem> Items { get; set; } = [];
    public ICollection<Payment> Payments { get; set; } = [];
}
