namespace Vinsaro.Domain.Entities;

/// <summary>
/// Immutable snapshot of a product variant at the time of purchase.
/// Product name, SKU, color, size, and unit price are frozen at order placement.
/// Never recalculated from the current product table.
/// </summary>
public class OrderItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;

    // FK kept for reference, but the snapshot fields below are the source of truth for this order
    public Guid? ProductVariantId { get; set; }
    public ProductVariant? ProductVariant { get; set; }

    // Immutable purchase-time snapshot
    public string ProductName { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public string? SelectedColor { get; set; }
    public string? SelectedSize { get; set; }

    /// <summary>Unit price at time of purchase (BDT, NUMERIC(18,2)).</summary>
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal TotalPrice { get; set; }

    // Allows customers with delivered items to leave one review per order item
    public ProductReview? Review { get; set; }
}
