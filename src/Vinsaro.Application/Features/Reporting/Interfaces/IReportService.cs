using Vinsaro.Application.Common;
using Vinsaro.Application.Features.Reporting.DTOs;

namespace Vinsaro.Application.Features.Reporting.Interfaces;

public interface IReportService
{
    Task<Result<SalesReportDto>> GetSalesReportAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
}
