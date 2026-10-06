using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vinsaro.Domain.Entities;
using Vinsaro.Infrastructure.Identity;

namespace Vinsaro.Infrastructure.Persistence.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("payments");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Amount).HasColumnType("numeric(18,2)");
        builder.Property(p => p.Currency).HasMaxLength(3).HasDefaultValue("BDT");
        builder.Property(p => p.PaymentMethod).HasConversion<string>().HasMaxLength(30);
        builder.Property(p => p.PaymentStatus).HasConversion<string>().HasMaxLength(20);
        builder.Property(p => p.ProviderTransactionId).HasMaxLength(200);
        builder.Property(p => p.ProviderReference).HasMaxLength(200);
        builder.HasIndex(p => p.OrderId);

        builder.HasOne(p => p.Order)
            .WithMany(o => o.Payments)
            .HasForeignKey(p => p.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class CouponConfiguration : IEntityTypeConfiguration<Coupon>
{
    public void Configure(EntityTypeBuilder<Coupon> builder)
    {
        builder.ToTable("coupons");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Code).IsRequired().HasMaxLength(50);
        builder.HasIndex(c => c.Code).IsUnique();
        builder.Property(c => c.DiscountType).HasConversion<string>().HasMaxLength(20);
        builder.Property(c => c.DiscountValue).HasColumnType("numeric(18,2)");
        builder.Property(c => c.MinimumOrderAmount).HasColumnType("numeric(18,2)");
        builder.Property(c => c.MaximumDiscountAmount).HasColumnType("numeric(18,2)");
    }
}

public class ProductReviewConfiguration : IEntityTypeConfiguration<ProductReview>
{
    public void Configure(EntityTypeBuilder<ProductReview> builder)
    {
        builder.ToTable("product_reviews");
        builder.HasKey(r => r.Id);

        // One review per order item (prevents duplicate reviews for same purchase)
        builder.HasIndex(r => r.OrderItemId).IsUnique();
        builder.HasIndex(r => new { r.ProductId, r.IsApproved });

        builder.HasOne(r => r.Product)
            .WithMany(p => p.Reviews)
            .HasForeignKey(r => r.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<ApplicationUser>()
            .WithMany(u => u.Reviews)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.OrderItem)
            .WithOne(i => i.Review)
            .HasForeignKey<ProductReview>(r => r.OrderItemId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class InventoryMovementConfiguration : IEntityTypeConfiguration<InventoryMovement>
{
    public void Configure(EntityTypeBuilder<InventoryMovement> builder)
    {
        builder.ToTable("inventory_movements");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.MovementType).HasConversion<string>().HasMaxLength(30);
        builder.Property(m => m.ReferenceType).HasMaxLength(50);
        builder.Property(m => m.Notes).HasMaxLength(500);
        builder.HasIndex(m => m.ProductVariantId);
        builder.HasIndex(m => new { m.ReferenceType, m.ReferenceId });

        builder.HasOne(m => m.ProductVariant)
            .WithMany(v => v.InventoryMovements)
            .HasForeignKey(m => m.ProductVariantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(m => m.CreatedByUserId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);
    }
}
