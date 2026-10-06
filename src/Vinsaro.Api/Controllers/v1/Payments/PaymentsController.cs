using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vinsaro.Application.Features.Payments.DTOs;
using Vinsaro.Application.Features.Payments.Interfaces;

namespace Vinsaro.Api.Controllers.v1.Payments;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentsController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
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
    public async Task<IActionResult> ProcessPayment([FromBody] ProcessPaymentRequest request, CancellationToken cancellationToken)
    {
        var result = await _paymentService.ProcessPaymentAsync(GetUserId(), request, cancellationToken);
        if (!result.IsSuccess) return BadRequest(new { Error = result.Error });
        return Ok(result.Value);
    }

    [HttpGet("order/{orderId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrderPayments(Guid orderId, CancellationToken cancellationToken)
    {
        // Ideally should verify if the user owns the order, simplified for now
        var result = await _paymentService.GetOrderPaymentsAsync(orderId, cancellationToken);
        return Ok(result.Value);
    }

    // Admin endpoints

    [HttpPut("{id:guid}/status")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdatePaymentStatusRequest request, CancellationToken cancellationToken)
    {
        var result = await _paymentService.UpdatePaymentStatusAsync(id, request, cancellationToken);
        if (!result.IsSuccess) return BadRequest(new { Error = result.Error });
        return Ok(result.Value);
    }
}
