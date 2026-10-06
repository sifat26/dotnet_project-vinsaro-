namespace Vinsaro.Application.Features.Reporting.DTOs;

public record SalesSummaryDto(
    decimal TotalSales,
    int TotalOrders,
    decimal AverageOrderValue
);

public record TopProductDto(
    Guid ProductVariantId,
    string ProductName,
    string Sku,
    int TotalQuantitySold,
    decimal TotalRevenue
);

public record SalesReportDto(
    DateTime StartDate,
    DateTime EndDate,
    SalesSummaryDto Summary,
    List<TopProductDto> TopProducts
);
