using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vinsaro.Application.Features.Reporting.Interfaces;

namespace Vinsaro.Api.Controllers.v1.Reporting;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize(Roles = "Admin")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("sales")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSalesReport(
        [FromQuery] DateTime? startDate, 
        [FromQuery] DateTime? endDate, 
        CancellationToken cancellationToken)
    {
        var start = startDate ?? DateTime.UtcNow.AddDays(-30);
        var end = endDate ?? DateTime.UtcNow;

        var result = await _reportService.GetSalesReportAsync(start, end, cancellationToken);
        return Ok(result.Value);
    }
}
