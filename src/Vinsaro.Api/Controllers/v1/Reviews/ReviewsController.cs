using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vinsaro.Application.Features.Reviews.DTOs;
using Vinsaro.Application.Features.Reviews.Interfaces;

namespace Vinsaro.Api.Controllers.v1.Reviews;

[ApiController]
[Route("api/v1/[controller]")]
public class ReviewsController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public ReviewsController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    private Guid GetUserId()
    {
        var idClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
        if (idClaim != null && Guid.TryParse(idClaim.Value, out var userId))
            return userId;
        return Guid.Empty;
    }

    [HttpGet("product/{productId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProductReviews(Guid productId, CancellationToken cancellationToken)
    {
        // Anyone can read approved reviews
        var result = await _reviewService.GetProductReviewsAsync(productId, cancellationToken);
        return Ok(result.Value);
    }

    [HttpPost]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateReviewRequest request, CancellationToken cancellationToken)
    {
        var result = await _reviewService.CreateReviewAsync(GetUserId(), request, cancellationToken);
        if (!result.IsSuccess) return BadRequest(new { Error = result.Error });
        return Ok(result.Value);
    }

    // Admin endpoints

    [HttpGet("pending")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPendingReviews(CancellationToken cancellationToken)
    {
        var result = await _reviewService.GetPendingReviewsAsync(cancellationToken);
        return Ok(result.Value);
    }

    [HttpPut("{id:guid}/approve")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ApproveReview(Guid id, CancellationToken cancellationToken)
    {
        var result = await _reviewService.ApproveReviewAsync(id, cancellationToken);
        if (!result.IsSuccess) return BadRequest(new { Error = result.Error });
        return NoContent();
    }
}
