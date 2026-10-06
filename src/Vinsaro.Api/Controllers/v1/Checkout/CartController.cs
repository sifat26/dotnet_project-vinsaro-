using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vinsaro.Application.Features.Checkout.DTOs;
using Vinsaro.Application.Features.Checkout.Interfaces;

namespace Vinsaro.Api.Controllers.v1.Checkout;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }

    private Guid GetUserId()
    {
        var idClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
        if (idClaim != null && Guid.TryParse(idClaim.Value, out var userId))
            return userId;
        return Guid.Empty;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCart(CancellationToken cancellationToken)
    {
        var result = await _cartService.GetCartAsync(GetUserId(), cancellationToken);
        if (!result.IsSuccess) return BadRequest(new { Error = result.Error });
        return Ok(result.Value);
    }

    [HttpPost("items")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddItem([FromBody] AddToCartRequest request, CancellationToken cancellationToken)
    {
        var result = await _cartService.AddToCartAsync(GetUserId(), request, cancellationToken);
        if (!result.IsSuccess) return BadRequest(new { Error = result.Error });
        return Ok(result.Value);
    }

    [HttpPut("items/{cartItemId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateItem(Guid cartItemId, [FromBody] UpdateCartItemRequest request, CancellationToken cancellationToken)
    {
        var result = await _cartService.UpdateCartItemAsync(GetUserId(), cartItemId, request, cancellationToken);
        if (!result.IsSuccess) return BadRequest(new { Error = result.Error });
        return Ok(result.Value);
    }

    [HttpDelete("items/{cartItemId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RemoveItem(Guid cartItemId, CancellationToken cancellationToken)
    {
        var result = await _cartService.RemoveFromCartAsync(GetUserId(), cartItemId, cancellationToken);
        if (!result.IsSuccess) return BadRequest(new { Error = result.Error });
        return Ok(result.Value);
    }

    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ClearCart(CancellationToken cancellationToken)
    {
        var result = await _cartService.ClearCartAsync(GetUserId(), cancellationToken);
        if (!result.IsSuccess) return BadRequest(new { Error = result.Error });
        return NoContent();
    }
}
