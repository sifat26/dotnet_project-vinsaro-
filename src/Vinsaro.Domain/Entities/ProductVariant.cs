using Vinsaro.Domain.Common;

namespace Vinsaro.Domain.Entities;

/// <summary>
/// A purchasable variant of a product (e.g., Black / M). 
/// Price is stored as PostgreSQL NUMERIC → C# decimal for accurate monetary representation.
/// </summary>
public class ProductVariant : BaseEntity
{
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public string SKU { get; set; } = string.Empty;
    public string? Color { get; set; }
    public string? Size { get; set; }

    /// <summary>Price in BDT. Stored as NUMERIC(18,2) — never float/double.</summary>
    public decimal Price { get; set; }

    public int StockQuantity { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<CartItem> CartItems { get; set; } = [];
    public ICollection<OrderItem> OrderItems { get; set; } = [];
    public ICollection<InventoryMovement> InventoryMovements { get; set; } = [];
}
