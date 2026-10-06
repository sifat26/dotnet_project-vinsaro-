using Microsoft.EntityFrameworkCore;
using Vinsaro.Application.Common;
using Vinsaro.Application.Features.Checkout.DTOs;
using Vinsaro.Application.Features.Checkout.Interfaces;
using Vinsaro.Domain.Entities;
using Vinsaro.Infrastructure.Persistence;

namespace Vinsaro.Infrastructure.Checkout.Services;

public class CartService : ICartService
{
    private readonly VinsaroDbContext _dbContext;

    public CartService(VinsaroDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<CartDto>> GetCartAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var cart = await GetOrCreateCartAsync(userId, cancellationToken);
        return Result<CartDto>.Success(MapToDto(cart));
    }

    public async Task<Result<CartDto>> AddToCartAsync(Guid userId, AddToCartRequest request, CancellationToken cancellationToken = default)
    {
        var variant = await _dbContext.ProductVariants
            .Include(v => v.Product)
            .ThenInclude(p => p.Images)
            .FirstOrDefaultAsync(v => v.Id == request.ProductVariantId && v.IsActive, cancellationToken);

        if (variant == null) return Result<CartDto>.Failure("Product variant not found or inactive.");

        if (variant.StockQuantity < request.Quantity)
            return Result<CartDto>.Failure($"Insufficient stock. Only {variant.StockQuantity} available.");

        var cart = await GetOrCreateCartAsync(userId, cancellationToken);

        var existingItem = cart.Items.FirstOrDefault(i => i.ProductVariantId == request.ProductVariantId);
        if (existingItem != null)
        {
            var newQuantity = existingItem.Quantity + request.Quantity;
            if (variant.StockQuantity < newQuantity)
                return Result<CartDto>.Failure($"Insufficient stock. Cannot add {request.Quantity} more to cart.");
            
            existingItem.Quantity = newQuantity;
        }
        else
        {
            var cartItem = new CartItem
            {
                CartId = cart.Id,
                ProductVariantId = request.ProductVariantId,
                Quantity = request.Quantity
            };
            _dbContext.CartItems.Add(cartItem);
            cart.Items.Add(cartItem);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        // Reload fully to get proper mapping
        cart = await GetOrCreateCartAsync(userId, cancellationToken);
        return Result<CartDto>.Success(MapToDto(cart));
    }

    public async Task<Result<CartDto>> UpdateCartItemAsync(Guid userId, Guid cartItemId, UpdateCartItemRequest request, CancellationToken cancellationToken = default)
    {
        var cart = await GetOrCreateCartAsync(userId, cancellationToken);
        var item = cart.Items.FirstOrDefault(i => i.Id == cartItemId);
        
        if (item == null) return Result<CartDto>.Failure("Cart item not found.");

        if (request.Quantity <= 0)
        {
            return await RemoveFromCartAsync(userId, cartItemId, cancellationToken);
        }

        var variant = await _dbContext.ProductVariants.FirstOrDefaultAsync(v => v.Id == item.ProductVariantId, cancellationToken);
        if (variant == null) return Result<CartDto>.Failure("Variant not found.");

        if (variant.StockQuantity < request.Quantity)
            return Result<CartDto>.Failure($"Insufficient stock. Only {variant.StockQuantity} available.");

        item.Quantity = request.Quantity;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result<CartDto>.Success(MapToDto(cart));
    }

    public async Task<Result<CartDto>> RemoveFromCartAsync(Guid userId, Guid cartItemId, CancellationToken cancellationToken = default)
    {
        var cart = await GetOrCreateCartAsync(userId, cancellationToken);
        var item = cart.Items.FirstOrDefault(i => i.Id == cartItemId);

        if (item != null)
        {
            _dbContext.CartItems.Remove(item);
            cart.Items.Remove(item);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return Result<CartDto>.Success(MapToDto(cart));
    }

    public async Task<Result<bool>> ClearCartAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var cart = await _dbContext.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);

        if (cart != null && cart.Items.Any())
        {
            _dbContext.CartItems.RemoveRange(cart.Items);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return Result<bool>.Success(true);
    }

    private async Task<Cart> GetOrCreateCartAsync(Guid userId, CancellationToken cancellationToken)
    {
        var cart = await _dbContext.Carts
            .Include(c => c.Items)
            .ThenInclude(i => i.ProductVariant)
            .ThenInclude(v => v.Product)
            .ThenInclude(p => p.Images)
            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);

        if (cart == null)
        {
            cart = new Cart { UserId = userId };
            _dbContext.Carts.Add(cart);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return cart;
    }

    private static CartDto MapToDto(Cart cart)
    {
        var items = cart.Items.Select(i => {
            var variant = i.ProductVariant;
            var product = variant?.Product;
            var primaryImage = product?.Images?.FirstOrDefault(img => img.IsPrimary)?.ImageUrl 
                            ?? product?.Images?.FirstOrDefault()?.ImageUrl;

            return new CartItemDto(
                i.Id,
                i.ProductVariantId,
                product?.Name ?? "Unknown Product",
                primaryImage,
                variant?.SKU ?? "",
                variant?.Size,
                variant?.Color,
                variant?.Price ?? 0m,
                i.Quantity,
                (variant?.Price ?? 0m) * i.Quantity
            );
        }).ToList();

        var subtotal = items.Sum(i => i.TotalPrice);

        return new CartDto(cart.Id, cart.UserId, items, subtotal);
    }
}
