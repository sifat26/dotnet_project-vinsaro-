using Microsoft.EntityFrameworkCore;
using Vinsaro.Application.Common;
using Vinsaro.Application.Features.Catalog.DTOs;
using Vinsaro.Application.Features.Catalog.Interfaces;
using Vinsaro.Domain.Entities;
using Vinsaro.Infrastructure.Persistence;

namespace Vinsaro.Infrastructure.Catalog.Services;

public class ProductService : IProductService
{
    private readonly VinsaroDbContext _dbContext;

    public ProductService(VinsaroDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<ProductDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await _dbContext.Products
            .Include(p => p.Category)
            .Include(p => p.Variants)
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (product == null) return Result<ProductDto>.Failure("Product not found.");

        return Result<ProductDto>.Success(MapToDto(product));
    }

    public async Task<Result<ProductDto>> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        var product = await _dbContext.Products
            .Include(p => p.Category)
            .Include(p => p.Variants)
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Slug == slug, cancellationToken);

        if (product == null) return Result<ProductDto>.Failure("Product not found.");

        return Result<ProductDto>.Success(MapToDto(product));
    }

    public async Task<Result<PagedResult<ProductDto>>> SearchAsync(ProductSearchFilter filter, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Products
            .Include(p => p.Category)
            .Include(p => p.Variants)
            .Include(p => p.Images)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var search = filter.SearchTerm.ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(search) || (p.Description != null && p.Description.ToLower().Contains(search)));
        }

        if (filter.CategoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == filter.CategoryId.Value);
        }

        if (filter.IsActive.HasValue)
        {
            query = query.Where(p => p.IsActive == filter.IsActive.Value);
        }

        if (filter.MinPrice.HasValue)
        {
            query = query.Where(p => p.Variants.Any() && p.Variants.Min(v => v.Price) >= filter.MinPrice.Value);
        }

        if (filter.MaxPrice.HasValue)
        {
            query = query.Where(p => p.Variants.Any() && p.Variants.Max(v => v.Price) <= filter.MaxPrice.Value);
        }

        var totalItems = await query.CountAsync(cancellationToken);

        query = filter.SortBy?.ToLower() switch
        {
            "price" => filter.SortDescending ? query.OrderByDescending(p => p.Variants.Min(v => v.Price)) : query.OrderBy(p => p.Variants.Min(v => v.Price)),
            "name" => filter.SortDescending ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
            "newest" => filter.SortDescending ? query.OrderByDescending(p => p.CreatedAt) : query.OrderBy(p => p.CreatedAt),
            _ => query.OrderBy(p => p.Name)
        };

        var items = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(p => MapToDto(p))
            .ToListAsync(cancellationToken);

        return Result<PagedResult<ProductDto>>.Success(new PagedResult<ProductDto> 
        { 
            Items = items, 
            TotalCount = totalItems, 
            PageNumber = filter.Page, 
            PageSize = filter.PageSize 
        });
    }

    public async Task<Result<ProductDto>> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default)
    {
        if (await _dbContext.Products.AnyAsync(p => p.Slug == request.Slug, cancellationToken))
            return Result<ProductDto>.Failure("A product with this slug already exists.");

        var categoryExists = await _dbContext.Categories.AnyAsync(c => c.Id == request.CategoryId, cancellationToken);
        if (!categoryExists) return Result<ProductDto>.Failure("Category not found.");

        var product = new Product
        {
            Name = request.Name,
            Slug = request.Slug,
            Description = request.Description,
            CategoryId = request.CategoryId,
            IsActive = request.IsActive,
            Variants = request.Variants.Select(v => new ProductVariant
            {
                SKU = v.Sku,
                Size = v.Size,
                Color = v.Color,
                Price = v.Price,
                StockQuantity = v.StockQuantity
            }).ToList(),
            Images = request.Images.Select(i => new ProductImage
            {
                ImageUrl = i.ImageUrl,
                SortOrder = i.SortOrder,
                IsPrimary = i.IsPrimary
            }).ToList()
        };

        _dbContext.Products.Add(product);
        await _dbContext.SaveChangesAsync(cancellationToken);

        // Reload to get Category Name
        return await GetByIdAsync(product.Id, cancellationToken);
    }

    public async Task<Result<ProductDto>> UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken cancellationToken = default)
    {
        var product = await _dbContext.Products.FindAsync(new object[] { id }, cancellationToken);
        if (product == null) return Result<ProductDto>.Failure("Product not found.");

        if (product.Slug != request.Slug && await _dbContext.Products.AnyAsync(p => p.Slug == request.Slug, cancellationToken))
            return Result<ProductDto>.Failure("A product with this slug already exists.");

        product.Name = request.Name;
        product.Slug = request.Slug;
        product.Description = request.Description;
        product.CategoryId = request.CategoryId;
        product.IsActive = request.IsActive;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<Result<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await _dbContext.Products.FindAsync(new object[] { id }, cancellationToken);
        if (product == null) return Result<bool>.Failure("Product not found.");

        _dbContext.Products.Remove(product);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }

    public async Task<Result<ProductVariantDto>> AddVariantAsync(Guid productId, CreateProductVariantRequest request, CancellationToken cancellationToken = default)
    {
        var productExists = await _dbContext.Products.AnyAsync(p => p.Id == productId, cancellationToken);
        if (!productExists) return Result<ProductVariantDto>.Failure("Product not found.");

        var variant = new ProductVariant
        {
            ProductId = productId,
            SKU = request.Sku,
            Size = request.Size,
            Color = request.Color,
            Price = request.Price,
            StockQuantity = request.StockQuantity
        };

        _dbContext.ProductVariants.Add(variant);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result<ProductVariantDto>.Success(new ProductVariantDto(
            variant.Id, variant.SKU, variant.Size, variant.Color, variant.Price, variant.StockQuantity));
    }

    public async Task<Result<bool>> DeleteVariantAsync(Guid productId, Guid variantId, CancellationToken cancellationToken = default)
    {
        var variant = await _dbContext.ProductVariants.FirstOrDefaultAsync(v => v.ProductId == productId && v.Id == variantId, cancellationToken);
        if (variant == null) return Result<bool>.Failure("Variant not found.");

        _dbContext.ProductVariants.Remove(variant);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }

    public async Task<Result<ProductImageDto>> AddImageAsync(Guid productId, CreateProductImageRequest request, CancellationToken cancellationToken = default)
    {
        var productExists = await _dbContext.Products.AnyAsync(p => p.Id == productId, cancellationToken);
        if (!productExists) return Result<ProductImageDto>.Failure("Product not found.");

        var image = new ProductImage
        {
            ProductId = productId,
            ImageUrl = request.ImageUrl,
            SortOrder = request.SortOrder,
            IsPrimary = request.IsPrimary
        };

        // If this is primary, unset others
        if (image.IsPrimary)
        {
            var otherImages = await _dbContext.ProductImages.Where(i => i.ProductId == productId).ToListAsync(cancellationToken);
            foreach (var img in otherImages) img.IsPrimary = false;
        }

        _dbContext.ProductImages.Add(image);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result<ProductImageDto>.Success(new ProductImageDto(
            image.Id, image.ImageUrl, image.SortOrder, image.IsPrimary));
    }

    public async Task<Result<bool>> DeleteImageAsync(Guid productId, Guid imageId, CancellationToken cancellationToken = default)
    {
        var image = await _dbContext.ProductImages.FirstOrDefaultAsync(i => i.ProductId == productId && i.Id == imageId, cancellationToken);
        if (image == null) return Result<bool>.Failure("Image not found.");

        _dbContext.ProductImages.Remove(image);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }

    private static ProductDto MapToDto(Product p)
    {
        return new ProductDto(
            p.Id,
            p.Name,
            p.Slug,
            p.Description,
            p.CategoryId,
            p.Category?.Name ?? string.Empty,
            p.IsActive,
            p.Variants.Select(v => new ProductVariantDto(v.Id, v.SKU, v.Size, v.Color, v.Price, v.StockQuantity)).ToList(),
            p.Images.OrderBy(i => i.SortOrder).Select(i => new ProductImageDto(i.Id, i.ImageUrl, i.SortOrder, i.IsPrimary)).ToList()
        );
    }
}
