using Microsoft.AspNetCore.Mvc;

namespace Vinsaro.Api.Controllers.v1;

/// <summary>Basic API info endpoint.</summary>
[ApiController]
[Route("api/v1")]
public class InfoController : ControllerBase
{
    /// <summary>Returns basic API status. Use /health for the ASP.NET health endpoint.</summary>
    [HttpGet("info")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetInfo()
    {
        return Ok(new
        {
            Service = "Vinsaro API",
            Version = "v1",
            Currency = "BDT",
            Timestamp = DateTime.UtcNow
        });
    }
}
