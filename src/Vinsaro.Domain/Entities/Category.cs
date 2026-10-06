using Vinsaro.Domain.Common;

namespace Vinsaro.Domain.Entities;

/// <summary>
/// Product category. Supports hierarchical structure via optional ParentCategoryId.
/// </summary>
public class Category : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; } = true;

    // Hierarchical support: null means top-level category
    public Guid? ParentCategoryId { get; set; }
    public Category? ParentCategory { get; set; }
    public ICollection<Category> SubCategories { get; set; } = [];

    public ICollection<Product> Products { get; set; } = [];
}
