using Microsoft.EntityFrameworkCore;
using Vinsaro.Application.Common;
using Vinsaro.Application.Features.Checkout.DTOs;
using Vinsaro.Application.Features.Checkout.Interfaces;
using Vinsaro.Domain.Entities;
using Vinsaro.Domain.Enums;
using Vinsaro.Infrastructure.Persistence;

namespace Vinsaro.Infrastructure.Checkout.Services;

public class OrderService : IOrderService
{
    private readonly VinsaroDbContext _dbContext;

    public OrderService(VinsaroDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<OrderDto>> GetOrderByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var order = await _dbContext.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

        if (order == null) return Result<OrderDto>.Failure("Order not found.");

        return Result<OrderDto>.Success(MapToDto(order));
    }

    public async Task<Result<OrderDto>> GetOrderByNumberAsync(string orderNumber, CancellationToken cancellationToken = default)
    {
        var order = await _dbContext.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber, cancellationToken);

        if (order == null) return Result<OrderDto>.Failure("Order not found.");

        return Result<OrderDto>.Success(MapToDto(order));
    }

    public async Task<Result<PagedResult<OrderDto>>> SearchOrdersAsync(OrderSearchFilter filter, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Orders
            .Include(o => o.Items)
            .AsQueryable();

        if (filter.UserId.HasValue)
            query = query.Where(o => o.UserId == filter.UserId.Value);

        if (filter.OrderStatus.HasValue)
            query = query.Where(o => o.OrderStatus == filter.OrderStatus.Value);

        if (filter.PaymentStatus.HasValue)
            query = query.Where(o => o.PaymentStatus == filter.PaymentStatus.Value);

        if (filter.StartDate.HasValue)
            query = query.Where(o => o.CreatedAt >= filter.StartDate.Value);

        if (filter.EndDate.HasValue)
            query = query.Where(o => o.CreatedAt <= filter.EndDate.Value);

        var totalItems = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(o => MapToDto(o))
            .ToListAsync(cancellationToken);

        return Result<PagedResult<OrderDto>>.Success(new PagedResult<OrderDto>
        {
            Items = items,
            TotalCount = totalItems,
            PageNumber = filter.Page,
            PageSize = filter.PageSize
        });
    }

    public async Task<Result<OrderDto>> UpdateOrderStatusAsync(Guid orderId, OrderStatus status, CancellationToken cancellationToken = default)
    {
        var order = await _dbContext.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);

        if (order == null) return Result<OrderDto>.Failure("Order not found.");

        // Handled cancelled order stock restoration
        if (status == OrderStatus.Cancelled && !order.InventoryRestored)
        {
            foreach (var item in order.Items)
            {
                if (item.ProductVariantId.HasValue)
                {
                    var variant = await _dbContext.ProductVariants.FindAsync(new object[] { item.ProductVariantId.Value }, cancellationToken);
                    if (variant != null)
                    {
                        variant.StockQuantity += item.Quantity;
                    }
                }
            }
            order.InventoryRestored = true;
        }

        order.OrderStatus = status;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result<OrderDto>.Success(MapToDto(order));
    }

    private static OrderDto MapToDto(Order o)
    {
        return new OrderDto(
            o.Id, o.OrderNumber, o.UserId, o.OrderStatus, o.PaymentStatus,
            o.Subtotal, o.DiscountAmount, o.DeliveryCharge, o.TotalAmount,
            o.Currency, o.CustomerName, o.CustomerPhone,
            o.ShippingAddressLine1, o.ShippingAddressLine2, o.ShippingArea,
            o.ShippingDistrict, o.ShippingDivision, o.ShippingPostalCode,
            o.CustomerNotes, o.CreatedAt,
            o.Items.Select(i => new OrderItemDto(
                i.Id, i.ProductVariantId, i.ProductName, i.SKU, i.SelectedColor, i.SelectedSize,
                i.UnitPrice, i.Quantity, i.TotalPrice
            )).ToList()
        );
    }
}
