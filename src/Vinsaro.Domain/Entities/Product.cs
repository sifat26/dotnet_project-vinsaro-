using Vinsaro.Domain.Common;

namespace Vinsaro.Domain.Entities;

/// <summary>
/// A product in the Vinsaro catalog. Variants represent purchasable SKUs.
/// </summary>
public class Product : BaseEntity
{
    public Guid CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Brand { get; set; }
    public bool IsFeatured { get; set; } = false;
    public bool IsActive { get; set; } = true;

    public ICollection<ProductVariant> Variants { get; set; } = [];
    public ICollection<ProductImage> Images { get; set; } = [];
    public ICollection<ProductReview> Reviews { get; set; } = [];
}
