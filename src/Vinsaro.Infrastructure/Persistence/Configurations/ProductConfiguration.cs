using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vinsaro.Domain.Entities;

namespace Vinsaro.Infrastructure.Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("products");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name).IsRequired().HasMaxLength(200);
        builder.Property(p => p.Slug).IsRequired().HasMaxLength(220);
        builder.Property(p => p.Brand).HasMaxLength(100);

        // Unique slug for SEO-friendly URLs
        builder.HasIndex(p => p.Slug).IsUnique();
        builder.HasIndex(p => p.CategoryId);
        builder.HasIndex(p => new { p.IsActive, p.IsFeatured });

        builder.HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete — deactivate category instead
    }
}

public class ProductVariantConfiguration : IEntityTypeConfiguration<ProductVariant>
{
    public void Configure(EntityTypeBuilder<ProductVariant> builder)
    {
        builder.ToTable("product_variants");
        builder.HasKey(v => v.Id);

        builder.Property(v => v.SKU).IsRequired().HasMaxLength(100);
        builder.Property(v => v.Color).HasMaxLength(50);
        builder.Property(v => v.Size).HasMaxLength(20);

        // NUMERIC(18,2) for BDT monetary values — never float
        builder.Property(v => v.Price).HasColumnType("numeric(18,2)");

        builder.HasIndex(v => v.SKU).IsUnique();
        builder.HasIndex(v => v.ProductId);

        builder.HasOne(v => v.Product)
            .WithMany(p => p.Variants)
            .HasForeignKey(v => v.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
{
    public void Configure(EntityTypeBuilder<ProductImage> builder)
    {
        builder.ToTable("product_images");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.ImageUrl).IsRequired().HasMaxLength(500);
        builder.Property(i => i.StoragePath).HasMaxLength(500);
        builder.HasIndex(i => new { i.ProductId, i.IsPrimary });

        builder.HasOne(i => i.Product)
            .WithMany(p => p.Images)
            .HasForeignKey(i => i.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
