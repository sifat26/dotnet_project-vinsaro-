using Vinsaro.Application.Common;
using Vinsaro.Application.Features.Catalog.DTOs;

namespace Vinsaro.Application.Features.Catalog.Interfaces;

public interface IProductService
{
    Task<Result<ProductDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<ProductDto>> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<Result<PagedResult<ProductDto>>> SearchAsync(ProductSearchFilter filter, CancellationToken cancellationToken = default);
    
    Task<Result<ProductDto>> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default);
    Task<Result<ProductDto>> UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken cancellationToken = default);
    Task<Result<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Result<ProductVariantDto>> AddVariantAsync(Guid productId, CreateProductVariantRequest request, CancellationToken cancellationToken = default);
    Task<Result<bool>> DeleteVariantAsync(Guid productId, Guid variantId, CancellationToken cancellationToken = default);

    Task<Result<ProductImageDto>> AddImageAsync(Guid productId, CreateProductImageRequest request, CancellationToken cancellationToken = default);
    Task<Result<bool>> DeleteImageAsync(Guid productId, Guid imageId, CancellationToken cancellationToken = default);
}
