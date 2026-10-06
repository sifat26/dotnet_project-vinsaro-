using Vinsaro.Domain.Enums;

namespace Vinsaro.Application.Features.Catalog.DTOs;

public record CategoryDto(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    Guid? ParentCategoryId,
    List<CategoryDto>? SubCategories
);

public record CreateCategoryRequest(
    string Name,
    string Slug,
    string? Description,
    Guid? ParentCategoryId
);

public record UpdateCategoryRequest(
    string Name,
    string Slug,
    string? Description,
    Guid? ParentCategoryId
);
