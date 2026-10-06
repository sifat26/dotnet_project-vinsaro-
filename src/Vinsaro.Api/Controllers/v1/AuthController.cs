using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vinsaro.Application.Features.Auth.DTOs;
using Vinsaro.Application.Features.Auth.Interfaces;

namespace Vinsaro.Api.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IIdentityService _identityService;

    public AuthController(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        var result = await _identityService.RegisterAsync(request, cancellationToken);
        if (!result.IsSuccess)
        {
            return BadRequest(new { Error = result.Error });
        }

        return Ok(result.Value);
    }

    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = Request.Headers.UserAgent.ToString();

        var result = await _identityService.LoginAsync(request, ipAddress, userAgent, cancellationToken);
        if (!result.IsSuccess)
        {
            return BadRequest(new { Error = result.Error });
        }

        return Ok(result.Value);
    }

    [HttpPost("refresh-token")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = Request.Headers.UserAgent.ToString();

        var result = await _identityService.RefreshTokenAsync(request, ipAddress, userAgent, cancellationToken);
        if (!result.IsSuccess)
        {
            return BadRequest(new { Error = result.Error });
        }

        return Ok(result.Value);
    }

    [Authorize]
    [HttpPost("revoke-token")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RevokeToken([FromBody] string refreshToken, CancellationToken cancellationToken)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

        var result = await _identityService.RevokeTokenAsync(refreshToken, ipAddress, cancellationToken);
        if (!result.IsSuccess)
        {
            return BadRequest(new { Error = result.Error });
        }

        return Ok(new { Message = "Token revoked" });
    }

    [HttpPost("provision-admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ProvisionAdmin([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        var result = await _identityService.ProvisionInitialAdminAsync(request, cancellationToken);
        if (!result.IsSuccess)
        {
            return BadRequest(new { Error = result.Error });
        }

        return Ok(new { Message = "Admin provisioned successfully. You can now login." });
    }
}
