using Vinsaro.Domain.Common;

namespace Vinsaro.Domain.Entities;

/// <summary>
/// Customer review for a product. Only customers with a qualifying delivered purchase
/// for the specific order item may submit a review. One review per OrderItem.
/// </summary>
public class ProductReview : BaseEntity
{
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public Guid UserId { get; set; }

    /// <summary>
    /// The specific order item that qualifies this customer to review.
    /// Unique constraint prevents duplicate reviews for the same purchase.
    /// </summary>
    public Guid OrderItemId { get; set; }
    public OrderItem OrderItem { get; set; } = null!;

    /// <summary>Rating 1–5 inclusive.</summary>
    public int Rating { get; set; }
    public string? Comment { get; set; }

    /// <summary>Admin must approve reviews before they are publicly visible.</summary>
    public bool IsApproved { get; set; } = false;
}
