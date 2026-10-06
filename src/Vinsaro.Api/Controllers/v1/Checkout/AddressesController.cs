using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vinsaro.Application.Features.Checkout.DTOs;
using Vinsaro.Application.Features.Checkout.Interfaces;

namespace Vinsaro.Api.Controllers.v1.Checkout;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class AddressesController : ControllerBase
{
    private readonly ICustomerAddressService _addressService;

    public AddressesController(ICustomerAddressService addressService)
    {
        _addressService = addressService;
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
    public async Task<IActionResult> GetAddresses(CancellationToken cancellationToken)
    {
        var result = await _addressService.GetUserAddressesAsync(GetUserId(), cancellationToken);
        if (!result.IsSuccess) return BadRequest(new { Error = result.Error });
        return Ok(result.Value);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAddress(Guid id, CancellationToken cancellationToken)
    {
        var result = await _addressService.GetAddressAsync(GetUserId(), id, cancellationToken);
        if (!result.IsSuccess) return NotFound(new { Error = result.Error });
        return Ok(result.Value);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateCustomerAddressRequest request, CancellationToken cancellationToken)
    {
        var result = await _addressService.CreateAddressAsync(GetUserId(), request, cancellationToken);
        if (!result.IsSuccess) return BadRequest(new { Error = result.Error });
        return CreatedAtAction(nameof(GetAddress), new { id = result.Value!.Id }, result.Value);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCustomerAddressRequest request, CancellationToken cancellationToken)
    {
        var result = await _addressService.UpdateAddressAsync(GetUserId(), id, request, cancellationToken);
        if (!result.IsSuccess) return BadRequest(new { Error = result.Error });
        return Ok(result.Value);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _addressService.DeleteAddressAsync(GetUserId(), id, cancellationToken);
        if (!result.IsSuccess) return BadRequest(new { Error = result.Error });
        return NoContent();
    }

    [HttpPost("{id:guid}/default")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SetDefault(Guid id, CancellationToken cancellationToken)
    {
        var result = await _addressService.SetDefaultAddressAsync(GetUserId(), id, cancellationToken);
        if (!result.IsSuccess) return BadRequest(new { Error = result.Error });
        return Ok(result.Value);
    }
}
