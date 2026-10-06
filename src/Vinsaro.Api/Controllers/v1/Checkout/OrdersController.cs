using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vinsaro.Application.Features.Checkout.DTOs;
using Vinsaro.Application.Features.Checkout.Interfaces;
using Vinsaro.Domain.Enums;

namespace Vinsaro.Api.Controllers.v1.Checkout;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
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
    public async Task<IActionResult> GetMyOrders([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var filter = new OrderSearchFilter(GetUserId(), null, null, null, null, page, pageSize);
        var result = await _orderService.SearchOrdersAsync(filter, cancellationToken);
        return Ok(result.Value);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrderById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _orderService.GetOrderByIdAsync(id, cancellationToken);
        if (!result.IsSuccess) return NotFound(new { Error = result.Error });
        
        // Ensure users can only fetch their own orders, unless they are Admin
        if (result.Value!.UserId != GetUserId() && !User.IsInRole("Admin"))
            return Forbid();

        return Ok(result.Value);
    }

    [HttpGet("number/{orderNumber}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrderByNumber(string orderNumber, CancellationToken cancellationToken)
    {
        var result = await _orderService.GetOrderByNumberAsync(orderNumber, cancellationToken);
        if (!result.IsSuccess) return NotFound(new { Error = result.Error });
        
        // Ensure users can only fetch their own orders, unless they are Admin
        if (result.Value!.UserId != GetUserId() && !User.IsInRole("Admin"))
            return Forbid();

        return Ok(result.Value);
    }

    // Admin endpoints

    [HttpGet("all")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchAllOrders([FromQuery] OrderSearchFilter filter, CancellationToken cancellationToken)
    {
        var result = await _orderService.SearchOrdersAsync(filter, cancellationToken);
        return Ok(result.Value);
    }

    [HttpPut("{id:guid}/status")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateOrderStatus(Guid id, [FromBody] OrderStatus status, CancellationToken cancellationToken)
    {
        var result = await _orderService.UpdateOrderStatusAsync(id, status, cancellationToken);
        if (!result.IsSuccess) return BadRequest(new { Error = result.Error });
        return Ok(result.Value);
    }
}
