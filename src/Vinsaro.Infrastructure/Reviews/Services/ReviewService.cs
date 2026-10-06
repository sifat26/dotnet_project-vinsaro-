using Microsoft.EntityFrameworkCore;
using Vinsaro.Application.Common;
using Vinsaro.Application.Features.Reviews.DTOs;
using Vinsaro.Application.Features.Reviews.Interfaces;
using Vinsaro.Domain.Entities;
using Vinsaro.Infrastructure.Persistence;

namespace Vinsaro.Infrastructure.Reviews.Services;

public class ReviewService : IReviewService
{
    private readonly VinsaroDbContext _dbContext;

    public ReviewService(VinsaroDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<ProductReviewDto>> CreateReviewAsync(Guid userId, CreateReviewRequest request, CancellationToken cancellationToken = default)
    {
        var orderItem = await _dbContext.OrderItems
            .Include(oi => oi.Order)
            .Include(oi => oi.ProductVariant)
            .FirstOrDefaultAsync(oi => oi.Id == request.OrderItemId && oi.Order.UserId == userId, cancellationToken);

        if (orderItem == null) return Result<ProductReviewDto>.Failure("Order item not found or you don't have permission.");

        // In a real app, you might want to enforce that OrderStatus == Delivered
        // if (orderItem.Order.OrderStatus != Vinsaro.Domain.Enums.OrderStatus.Delivered)
        //    return Result<ProductReviewDto>.Failure("You can only review delivered items.");

        if (await _dbContext.ProductReviews.AnyAsync(pr => pr.OrderItemId == request.OrderItemId, cancellationToken))
            return Result<ProductReviewDto>.Failure("You have already reviewed this item.");

        var productId = orderItem.ProductVariant?.ProductId;
        if (productId == null) return Result<ProductReviewDto>.Failure("Product information missing.");

        var review = new ProductReview
        {
            ProductId = productId.Value,
            UserId = userId,
            OrderItemId = request.OrderItemId,
            Rating = request.Rating,
            Comment = request.Comment,
            IsApproved = false // Requires Admin approval
        };

        _dbContext.ProductReviews.Add(review);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result<ProductReviewDto>.Success(MapToDto(review));
    }

    public async Task<Result<bool>> ApproveReviewAsync(Guid reviewId, CancellationToken cancellationToken = default)
    {
        var review = await _dbContext.ProductReviews.FindAsync(new object[] { reviewId }, cancellationToken);
        if (review == null) return Result<bool>.Failure("Review not found.");

        review.IsApproved = true;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }

    public async Task<Result<List<ProductReviewDto>>> GetProductReviewsAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        var reviews = await _dbContext.ProductReviews
            .Where(r => r.ProductId == productId && r.IsApproved)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);

        return Result<List<ProductReviewDto>>.Success(reviews.Select(MapToDto).ToList());
    }

    public async Task<Result<List<ProductReviewDto>>> GetPendingReviewsAsync(CancellationToken cancellationToken = default)
    {
        var reviews = await _dbContext.ProductReviews
            .Where(r => !r.IsApproved)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);

        return Result<List<ProductReviewDto>>.Success(reviews.Select(MapToDto).ToList());
    }

    private static ProductReviewDto MapToDto(ProductReview r)
    {
        return new ProductReviewDto(
            r.Id, r.ProductId, r.UserId, r.OrderItemId, r.Rating, r.Comment, r.IsApproved, r.CreatedAt
        );
    }
}
