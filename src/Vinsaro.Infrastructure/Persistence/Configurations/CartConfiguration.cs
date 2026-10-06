using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vinsaro.Domain.Entities;
using Vinsaro.Infrastructure.Identity;

namespace Vinsaro.Infrastructure.Persistence.Configurations;

public class CartConfiguration : IEntityTypeConfiguration<Cart>
{
    public void Configure(EntityTypeBuilder<Cart> builder)
    {
        builder.ToTable("carts");
        builder.HasKey(c => c.Id);
        // One cart per customer
        builder.HasIndex(c => c.UserId).IsUnique();

        builder.HasOne<ApplicationUser>()
            .WithOne(u => u.Cart)
            .HasForeignKey<Cart>(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder.ToTable("cart_items");
        builder.HasKey(i => i.Id);

        // Prevent duplicate rows for the same variant in the same cart
        builder.HasIndex(i => new { i.CartId, i.ProductVariantId }).IsUnique();

        builder.HasOne(i => i.Cart)
            .WithMany(c => c.Items)
            .HasForeignKey(i => i.CartId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(i => i.ProductVariant)
            .WithMany(v => v.CartItems)
            .HasForeignKey(i => i.ProductVariantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
