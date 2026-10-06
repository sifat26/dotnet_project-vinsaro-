using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vinsaro.Domain.Entities;
using Vinsaro.Infrastructure.Identity;

namespace Vinsaro.Infrastructure.Persistence.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("categories");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Name).IsRequired().HasMaxLength(100);
        builder.Property(c => c.Slug).IsRequired().HasMaxLength(120);
        builder.HasIndex(c => c.Slug).IsUnique();

        // Self-referencing hierarchy
        builder.HasOne(c => c.ParentCategory)
            .WithMany(c => c.SubCategories)
            .HasForeignKey(c => c.ParentCategoryId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);
    }
}

public class CustomerAddressConfiguration : IEntityTypeConfiguration<CustomerAddress>
{
    public void Configure(EntityTypeBuilder<CustomerAddress> builder)
    {
        builder.ToTable("customer_addresses");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.FullName).IsRequired().HasMaxLength(150);
        builder.Property(a => a.PhoneNumber).IsRequired().HasMaxLength(20);
        builder.Property(a => a.AddressLine1).IsRequired().HasMaxLength(255);
        builder.Property(a => a.AddressLine2).HasMaxLength(255);
        builder.Property(a => a.Area).IsRequired().HasMaxLength(100);
        builder.Property(a => a.District).IsRequired().HasMaxLength(100);
        builder.Property(a => a.Division).IsRequired().HasMaxLength(100);
        builder.Property(a => a.PostalCode).HasMaxLength(10);
        builder.HasIndex(a => a.UserId);

        builder.HasOne<ApplicationUser>()
            .WithMany(u => u.Addresses)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_tokens");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.TokenHash).IsRequired().HasMaxLength(500);
        builder.Property(r => r.IpAddress).HasMaxLength(45);
        builder.Property(r => r.UserAgent).HasMaxLength(500);
        builder.HasIndex(r => r.UserId);
        builder.HasIndex(r => new { r.UserId, r.IsRevoked });

        builder.HasOne(r => r.User)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
