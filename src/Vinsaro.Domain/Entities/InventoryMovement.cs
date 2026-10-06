using Vinsaro.Domain.Enums;

namespace Vinsaro.Domain.Entities;

/// <summary>
/// Audit log of inventory changes. Every stock change (purchase, cancellation, return, manual adjustment)
/// creates an immutable movement record for traceability.
/// </summary>
public class InventoryMovement
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProductVariantId { get; set; }
    public ProductVariant ProductVariant { get; set; } = null!;

    public MovementType MovementType { get; set; }

    /// <summary>Positive = stock added; negative = stock deducted.</summary>
    public int Quantity { get; set; }

    /// <summary>Type of the entity that caused this movement (e.g. "Order", "ManualAdjustment").</summary>
    public string? ReferenceType { get; set; }

    /// <summary>ID of the reference entity (e.g. OrderId).</summary>
    public Guid? ReferenceId { get; set; }

    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Guid? CreatedByUserId { get; set; }
}
