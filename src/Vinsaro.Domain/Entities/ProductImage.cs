namespace Vinsaro.Domain.Entities;

/// <summary>
/// Product image metadata. Binary content is stored in Supabase Storage — only the URL and path are persisted here.
/// </summary>
public class ProductImage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public string ImageUrl { get; set; } = string.Empty;

    /// <summary>Storage path in Supabase Storage bucket (e.g. "products/abc123.jpg").</summary>
    public string? StoragePath { get; set; }

    public bool IsPrimary { get; set; } = false;
    public int SortOrder { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
