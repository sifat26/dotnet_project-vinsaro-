using Vinsaro.Application.Common;
using Vinsaro.Application.Features.Reviews.DTOs;

namespace Vinsaro.Application.Features.Reviews.Interfaces;

public interface IReviewService
{
    Task<Result<ProductReviewDto>> CreateReviewAsync(Guid userId, CreateReviewRequest request, CancellationToken cancellationToken = default);
    Task<Result<bool>> ApproveReviewAsync(Guid reviewId, CancellationToken cancellationToken = default);
    Task<Result<List<ProductReviewDto>>> GetProductReviewsAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<Result<List<ProductReviewDto>>> GetPendingReviewsAsync(CancellationToken cancellationToken = default);
}
