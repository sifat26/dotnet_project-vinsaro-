using Microsoft.EntityFrameworkCore;
using Vinsaro.Application.Common;
using Vinsaro.Application.Features.Checkout.DTOs;
using Vinsaro.Application.Features.Checkout.Interfaces;
using Vinsaro.Domain.Entities;
using Vinsaro.Infrastructure.Persistence;

namespace Vinsaro.Infrastructure.Checkout.Services;

public class CheckoutService : ICheckoutService
{
    private readonly VinsaroDbContext _dbContext;

    public CheckoutService(VinsaroDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<CheckoutResponse>> ProcessCheckoutAsync(Guid userId, CheckoutRequest request, CancellationToken cancellationToken = default)
    {
        // 1. Idempotency Check
        if (!string.IsNullOrWhiteSpace(request.IdempotencyKey))
        {
            var existingOrder = await _dbContext.Orders
                .FirstOrDefaultAsync(o => o.IdempotencyKey == request.IdempotencyKey && o.UserId == userId, cancellationToken);
            
            if (existingOrder != null)
            {
                return Result<CheckoutResponse>.Success(new CheckoutResponse(existingOrder.Id, existingOrder.OrderNumber, existingOrder.TotalAmount));
            }
        }

        // 2. Fetch User & Address
        var user = await _dbContext.Users.FindAsync(new object[] { userId.ToString() }, cancellationToken);
        if (user == null) return Result<CheckoutResponse>.Failure("User not found.");

        var address = await _dbContext.CustomerAddresses.FirstOrDefaultAsync(a => a.Id == request.ShippingAddressId && a.UserId == userId, cancellationToken);
        if (address == null) return Result<CheckoutResponse>.Failure("Shipping address not found.");

        // 3. Fetch Cart & Validate Stock/Price
        var cart = await _dbContext.Carts
            .Include(c => c.Items)
            .ThenInclude(i => i.ProductVariant)
            .ThenInclude(v => v.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);

        if (cart == null || !cart.Items.Any()) return Result<CheckoutResponse>.Failure("Cart is empty.");

        var orderItems = new List<OrderItem>();
        decimal subtotal = 0;

        foreach (var item in cart.Items)
        {
            var variant = item.ProductVariant;
            if (variant == null || !variant.IsActive || !variant.Product.IsActive)
                return Result<CheckoutResponse>.Failure($"Product {item.ProductVariantId} is no longer available.");

            if (variant.StockQuantity < item.Quantity)
                return Result<CheckoutResponse>.Failure($"Insufficient stock for {variant.Product.Name}. Only {variant.StockQuantity} available.");

            // Deduct stock immediately to prevent overselling
            variant.StockQuantity -= item.Quantity;

            var totalPrice = variant.Price * item.Quantity;
            subtotal += totalPrice;

            orderItems.Add(new OrderItem
            {
                ProductVariantId = variant.Id,
                ProductName = variant.Product.Name,
                SKU = variant.SKU,
                SelectedColor = variant.Color,
                SelectedSize = variant.Size,
                UnitPrice = variant.Price,
                Quantity = item.Quantity,
                TotalPrice = totalPrice
            });
        }

        // 4. Calculate Delivery and Discounts (Hardcoded for now, could be dynamic later)
        decimal deliveryCharge = 100; // 100 BDT default delivery
        decimal discountAmount = 0;

        if (!string.IsNullOrWhiteSpace(request.CouponCode))
        {
            // Future: Implement coupon validation here
            // discountAmount = await _couponService.CalculateDiscount(request.CouponCode, subtotal);
        }

        decimal totalAmount = subtotal + deliveryCharge - discountAmount;

        // 5. Create Order Snapshot
        var order = new Order
        {
            UserId = userId,
            OrderNumber = $"VIN-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}",
            Subtotal = subtotal,
            DiscountAmount = discountAmount,
            DeliveryCharge = deliveryCharge,
            TotalAmount = totalAmount,
            Currency = "BDT",
            CustomerName = address.FullName,
            CustomerPhone = address.PhoneNumber,
            ShippingAddressLine1 = address.AddressLine1,
            ShippingAddressLine2 = address.AddressLine2,
            ShippingArea = address.Area,
            ShippingDistrict = address.District,
            ShippingDivision = address.Division,
            ShippingPostalCode = address.PostalCode,
            CustomerNotes = request.CustomerNotes,
            IdempotencyKey = request.IdempotencyKey,
            Items = orderItems
        };

        _dbContext.Orders.Add(order);

        // 6. Clear Cart
        _dbContext.CartItems.RemoveRange(cart.Items);

        // 7. Save Transaction
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result<CheckoutResponse>.Success(new CheckoutResponse(order.Id, order.OrderNumber, order.TotalAmount));
    }
}
