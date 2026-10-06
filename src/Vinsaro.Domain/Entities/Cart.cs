using Vinsaro.Domain.Common;

namespace Vinsaro.Domain.Entities;

/// <summary>
/// Shopping cart for a logged-in customer. One cart per customer at any time.
/// </summary>
public class Cart : BaseEntity
{
    public Guid UserId { get; set; }

    public ICollection<CartItem> Items { get; set; } = [];
}

/// <summary>
/// A line item in a cart. Enforces uniqueness on (CartId, ProductVariantId) at the DB level.
/// Server always re-validates prices and stock — never trusts client-supplied values.
/// </summary>
public class CartItem : BaseEntity
{
    public Guid CartId { get; set; }
    public Cart Cart { get; set; } = null!;

    public Guid ProductVariantId { get; set; }
    public ProductVariant ProductVariant { get; set; } = null!;

    public int Quantity { get; set; }
}
