using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vinsaro.Application.Features.Coupons.DTOs;
using Vinsaro.Application.Features.Coupons.Interfaces;

namespace Vinsaro.Api.Controllers.v1.Coupons;

[ApiController]
[Route("api/v1/[controller]")]
public class CouponsController : ControllerBase
{
    private readonly ICouponService _couponService;

    public CouponsController(ICouponService couponService)
    {
        _couponService = couponService;
    }

    [HttpGet("validate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ValidateCoupon([FromQuery] string code, [FromQuery] decimal subtotal, CancellationToken cancellationToken)
    {
        var result = await _couponService.ValidateAndCalculateDiscountAsync(code, subtotal, cancellationToken);
        return Ok(result.Value);
    }

    // Admin endpoints

    [HttpGet]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _couponService.GetAllCouponsAsync(cancellationToken);
        return Ok(result.Value);
    }

    [HttpGet("{code}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByCode(string code, CancellationToken cancellationToken)
    {
        var result = await _couponService.GetCouponByCodeAsync(code, cancellationToken);
        if (!result.IsSuccess) return NotFound(new { Error = result.Error });
        return Ok(result.Value);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateCouponRequest request, CancellationToken cancellationToken)
    {
        var result = await _couponService.CreateCouponAsync(request, cancellationToken);
        if (!result.IsSuccess) return BadRequest(new { Error = result.Error });
        return CreatedAtAction(nameof(GetByCode), new { code = result.Value!.Code }, result.Value);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _couponService.DeleteCouponAsync(id, cancellationToken);
        if (!result.IsSuccess) return BadRequest(new { Error = result.Error });
        return NoContent();
    }
}
