using Microsoft.EntityFrameworkCore;
using Vinsaro.Application.Common;
using Vinsaro.Application.Features.Catalog.DTOs;
using Vinsaro.Application.Features.Catalog.Interfaces;
using Vinsaro.Domain.Entities;
using Vinsaro.Infrastructure.Persistence;

namespace Vinsaro.Infrastructure.Catalog.Services;

public class CategoryService : ICategoryService
{
    private readonly VinsaroDbContext _dbContext;

    public CategoryService(VinsaroDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<CategoryDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var category = await _dbContext.Categories
            .Include(c => c.SubCategories)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (category == null)
            return Result<CategoryDto>.Failure("Category not found.");

        return Result<CategoryDto>.Success(MapToDto(category));
    }

    public async Task<Result<CategoryDto>> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        var category = await _dbContext.Categories
            .Include(c => c.SubCategories)
            .FirstOrDefaultAsync(c => c.Slug == slug, cancellationToken);

        if (category == null)
            return Result<CategoryDto>.Failure("Category not found.");

        return Result<CategoryDto>.Success(MapToDto(category));
    }

    public async Task<Result<List<CategoryDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var categories = await _dbContext.Categories
            .Include(c => c.SubCategories)
            .Where(c => c.ParentCategoryId == null) // Top level only
            .ToListAsync(cancellationToken);

        return Result<List<CategoryDto>>.Success(categories.Select(MapToDto).ToList());
    }

    public async Task<Result<CategoryDto>> CreateAsync(CreateCategoryRequest request, CancellationToken cancellationToken = default)
    {
        if (await _dbContext.Categories.AnyAsync(c => c.Slug == request.Slug, cancellationToken))
            return Result<CategoryDto>.Failure("A category with this slug already exists.");

        var category = new Category
        {
            Name = request.Name,
            Slug = request.Slug,
            Description = request.Description,
            ParentCategoryId = request.ParentCategoryId
        };

        _dbContext.Categories.Add(category);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result<CategoryDto>.Success(MapToDto(category));
    }

    public async Task<Result<CategoryDto>> UpdateAsync(Guid id, UpdateCategoryRequest request, CancellationToken cancellationToken = default)
    {
        var category = await _dbContext.Categories.FindAsync(new object[] { id }, cancellationToken);
        if (category == null)
            return Result<CategoryDto>.Failure("Category not found.");

        if (category.Slug != request.Slug && await _dbContext.Categories.AnyAsync(c => c.Slug == request.Slug, cancellationToken))
            return Result<CategoryDto>.Failure("A category with this slug already exists.");

        category.Name = request.Name;
        category.Slug = request.Slug;
        category.Description = request.Description;
        category.ParentCategoryId = request.ParentCategoryId;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result<CategoryDto>.Success(MapToDto(category));
    }

    public async Task<Result<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var category = await _dbContext.Categories
            .Include(c => c.SubCategories)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (category == null)
            return Result<bool>.Failure("Category not found.");

        if (category.SubCategories.Any())
            return Result<bool>.Failure("Cannot delete category with subcategories.");

        var hasProducts = await _dbContext.Products.AnyAsync(p => p.CategoryId == id, cancellationToken);
        if (hasProducts)
            return Result<bool>.Failure("Cannot delete category containing products.");

        _dbContext.Categories.Remove(category);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }

    private CategoryDto MapToDto(Category category)
    {
        return new CategoryDto(
            category.Id,
            category.Name,
            category.Slug,
            category.Description,
            category.ParentCategoryId,
            category.SubCategories?.Select(MapToDto).ToList()
        );
    }
}
