using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vinsaro.Application.Features.Checkout.DTOs;
using Vinsaro.Application.Features.Checkout.Interfaces;

namespace Vinsaro.Api.Controllers.v1.Checkout;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class CheckoutController : ControllerBase
{
    private readonly ICheckoutService _checkoutService;

    public CheckoutController(ICheckoutService checkoutService)
    {
        _checkoutService = checkoutService;
    }

    private Guid GetUserId()
    {
        var idClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
        if (idClaim != null && Guid.TryParse(idClaim.Value, out var userId))
            return userId;
        return Guid.Empty;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ProcessCheckout([FromBody] CheckoutRequest request, CancellationToken cancellationToken)
    {
        var result = await _checkoutService.ProcessCheckoutAsync(GetUserId(), request, cancellationToken);
        if (!result.IsSuccess) return BadRequest(new { Error = result.Error });
        return Ok(result.Value);
    }
}
