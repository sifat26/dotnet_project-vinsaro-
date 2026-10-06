using Vinsaro.Application.Common;
using Vinsaro.Application.Features.Catalog.DTOs;

namespace Vinsaro.Application.Features.Catalog.Interfaces;

public interface ICategoryService
{
    Task<Result<CategoryDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<CategoryDto>> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<Result<List<CategoryDto>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<CategoryDto>> CreateAsync(CreateCategoryRequest request, CancellationToken cancellationToken = default);
    Task<Result<CategoryDto>> UpdateAsync(Guid id, UpdateCategoryRequest request, CancellationToken cancellationToken = default);
    Task<Result<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
