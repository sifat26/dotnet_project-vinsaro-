

namespace Vinsaro.Application.Features.Catalog.DTOs;

public record ProductDto(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    Guid CategoryId,
    string CategoryName,
    bool IsActive,
    List<ProductVariantDto> Variants,
    List<ProductImageDto> Images
);

public record ProductVariantDto(
    Guid Id,
    string Sku,
    string? Size,
    string? Color,
    decimal Price,
    int StockQuantity
);

public record ProductImageDto(
    Guid Id,
    string ImageUrl,
    int SortOrder,
    bool IsPrimary
);

public record CreateProductRequest(
    string Name,
    string Slug,
    string? Description,
    Guid CategoryId,
    bool IsActive,
    List<CreateProductVariantRequest> Variants,
    List<CreateProductImageRequest> Images
);

public record CreateProductVariantRequest(
    string Sku,
    string? Size,
    string? Color,
    decimal Price,
    int StockQuantity
);

public record CreateProductImageRequest(
    string ImageUrl,
    int SortOrder,
    bool IsPrimary
);

public record UpdateProductRequest(
    string Name,
    string Slug,
    string? Description,
    Guid CategoryId,
    bool IsActive
);
