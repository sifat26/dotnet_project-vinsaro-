using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vinsaro.Domain.Entities;
using Vinsaro.Domain.Enums;
using Vinsaro.Infrastructure.Identity;

namespace Vinsaro.Infrastructure.Persistence.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("orders");
        builder.HasKey(o => o.Id);

        builder.Property(o => o.OrderNumber).IsRequired().HasMaxLength(30);
        builder.HasIndex(o => o.OrderNumber).IsUnique();
        builder.HasIndex(o => o.UserId);
        builder.HasIndex(o => o.OrderStatus);
        builder.HasIndex(o => o.IdempotencyKey).IsUnique().HasFilter("idempotency_key IS NOT NULL");

        // Monetary fields — NUMERIC(18,2) for BDT
        builder.Property(o => o.Subtotal).HasColumnType("numeric(18,2)");
        builder.Property(o => o.DiscountAmount).HasColumnType("numeric(18,2)");
        builder.Property(o => o.DeliveryCharge).HasColumnType("numeric(18,2)");
        builder.Property(o => o.TotalAmount).HasColumnType("numeric(18,2)");
        builder.Property(o => o.Currency).HasMaxLength(3).HasDefaultValue("BDT");

        // Enum storage as string for readability in DB
        builder.Property(o => o.OrderStatus).HasConversion<string>().HasMaxLength(20);
        builder.Property(o => o.PaymentStatus).HasConversion<string>().HasMaxLength(20);

        // Snapshot fields
        builder.Property(o => o.CustomerName).IsRequired().HasMaxLength(150);
        builder.Property(o => o.CustomerPhone).IsRequired().HasMaxLength(20);
        builder.Property(o => o.ShippingAddressLine1).IsRequired().HasMaxLength(255);
        builder.Property(o => o.ShippingArea).IsRequired().HasMaxLength(100);
        builder.Property(o => o.ShippingDistrict).IsRequired().HasMaxLength(100);
        builder.Property(o => o.ShippingDivision).IsRequired().HasMaxLength(100);

        builder.HasOne<ApplicationUser>()
            .WithMany(u => u.Orders)
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Restrict); // Keep orders even if user deleted

        builder.HasOne(o => o.Coupon)
            .WithMany(c => c.Orders)
            .HasForeignKey(o => o.CouponId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);
    }
}

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("order_items");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.ProductName).IsRequired().HasMaxLength(200);
        builder.Property(i => i.SKU).IsRequired().HasMaxLength(100);
        builder.Property(i => i.SelectedColor).HasMaxLength(50);
        builder.Property(i => i.SelectedSize).HasMaxLength(20);
        builder.Property(i => i.UnitPrice).HasColumnType("numeric(18,2)");
        builder.Property(i => i.TotalPrice).HasColumnType("numeric(18,2)");

        builder.HasOne(i => i.Order)
            .WithMany(o => o.Items)
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(i => i.ProductVariant)
            .WithMany(v => v.OrderItems)
            .HasForeignKey(i => i.ProductVariantId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false); // Variant may be deleted but order history must persist
    }
}
