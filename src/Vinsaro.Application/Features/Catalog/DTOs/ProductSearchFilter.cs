

namespace Vinsaro.Application.Features.Catalog.DTOs;

public record ProductSearchFilter(
    string? SearchTerm,
    Guid? CategoryId,
    bool? IsActive,
    decimal? MinPrice,
    decimal? MaxPrice,
    string? SortBy,
    bool SortDescending = false,
    int Page = 1,
    int PageSize = 20
);
