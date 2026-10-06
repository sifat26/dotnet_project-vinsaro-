namespace Vinsaro.Application.Features.Reviews.DTOs;

public record ProductReviewDto(
    Guid Id,
    Guid ProductId,
    Guid UserId,
    Guid OrderItemId,
    int Rating,
    string? Comment,
    bool IsApproved,
    DateTime CreatedAt
);

public record CreateReviewRequest(
    Guid OrderItemId,
    int Rating,
    string? Comment
);
