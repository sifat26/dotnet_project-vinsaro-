using Microsoft.EntityFrameworkCore;
using Vinsaro.Application.Common;
using Vinsaro.Application.Features.Reporting.DTOs;
using Vinsaro.Application.Features.Reporting.Interfaces;
using Vinsaro.Domain.Enums;
using Vinsaro.Infrastructure.Persistence;

namespace Vinsaro.Infrastructure.Reporting.Services;

public class ReportService : IReportService
{
    private readonly VinsaroDbContext _dbContext;

    public ReportService(VinsaroDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<SalesReportDto>> GetSalesReportAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        // Get paid orders in the date range
        var orders = await _dbContext.Orders
            .Include(o => o.Items)
            .Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate && o.PaymentStatus == PaymentStatus.Paid)
            .ToListAsync(cancellationToken);

        var totalOrders = orders.Count;
        var totalSales = orders.Sum(o => o.TotalAmount);
        var averageOrderValue = totalOrders > 0 ? totalSales / totalOrders : 0;

        var summary = new SalesSummaryDto(totalSales, totalOrders, averageOrderValue);

        // Calculate top products
        var allItems = orders.SelectMany(o => o.Items).ToList();
        
        var topProducts = allItems
            .Where(i => i.ProductVariantId.HasValue)
            .GroupBy(i => new { i.ProductVariantId, i.ProductName, i.SKU })
            .Select(g => new TopProductDto(
                g.Key.ProductVariantId!.Value,
                g.Key.ProductName,
                g.Key.SKU,
                g.Sum(i => i.Quantity),
                g.Sum(i => i.TotalPrice)
            ))
            .OrderByDescending(x => x.TotalRevenue)
            .Take(10)
            .ToList();

        var report = new SalesReportDto(startDate, endDate, summary, topProducts);

        return Result<SalesReportDto>.Success(report);
    }
}
