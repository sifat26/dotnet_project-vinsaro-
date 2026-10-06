using Microsoft.EntityFrameworkCore;
using Vinsaro.Application.Common;
using Vinsaro.Application.Features.Coupons.DTOs;
using Vinsaro.Application.Features.Coupons.Interfaces;
using Vinsaro.Domain.Entities;
using Vinsaro.Domain.Enums;
using Vinsaro.Infrastructure.Persistence;

namespace Vinsaro.Infrastructure.Coupons.Services;

public class CouponService : ICouponService
{
    private readonly VinsaroDbContext _dbContext;

    public CouponService(VinsaroDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<CouponDto>> CreateCouponAsync(CreateCouponRequest request, CancellationToken cancellationToken = default)
    {
        if (await _dbContext.Coupons.AnyAsync(c => c.Code == request.Code, cancellationToken))
            return Result<CouponDto>.Failure("Coupon code already exists.");

        var coupon = new Coupon
        {
            Code = request.Code,
            DiscountType = request.DiscountType,
            DiscountValue = request.DiscountValue,
            MinimumOrderAmount = request.MinimumOrderAmount,
            MaximumDiscountAmount = request.MaximumDiscountAmount,
            UsageLimit = request.UsageLimit,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            IsActive = request.IsActive
        };

        _dbContext.Coupons.Add(coupon);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result<CouponDto>.Success(MapToDto(coupon));
    }

    public async Task<Result<bool>> DeleteCouponAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var coupon = await _dbContext.Coupons.FindAsync(new object[] { id }, cancellationToken);
        if (coupon == null) return Result<bool>.Failure("Coupon not found.");

        _dbContext.Coupons.Remove(coupon);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }

    public async Task<Result<CouponDto>> GetCouponByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var coupon = await _dbContext.Coupons.FirstOrDefaultAsync(c => c.Code == code, cancellationToken);
        if (coupon == null) return Result<CouponDto>.Failure("Coupon not found.");

        return Result<CouponDto>.Success(MapToDto(coupon));
    }

    public async Task<Result<List<CouponDto>>> GetAllCouponsAsync(CancellationToken cancellationToken = default)
    {
        var coupons = await _dbContext.Coupons.ToListAsync(cancellationToken);
        return Result<List<CouponDto>>.Success(coupons.Select(MapToDto).ToList());
    }

    public async Task<Result<ValidateCouponResponse>> ValidateAndCalculateDiscountAsync(string code, decimal subtotal, CancellationToken cancellationToken = default)
    {
        var coupon = await _dbContext.Coupons.FirstOrDefaultAsync(c => c.Code == code, cancellationToken);

        if (coupon == null || !coupon.IsActive)
            return Result<ValidateCouponResponse>.Success(new ValidateCouponResponse(false, "Invalid or inactive coupon.", 0));

        if (coupon.StartDate.HasValue && coupon.StartDate.Value > DateTime.UtcNow)
            return Result<ValidateCouponResponse>.Success(new ValidateCouponResponse(false, "Coupon is not yet active.", 0));

        if (coupon.EndDate.HasValue && coupon.EndDate.Value < DateTime.UtcNow)
            return Result<ValidateCouponResponse>.Success(new ValidateCouponResponse(false, "Coupon has expired.", 0));

        if (coupon.UsageLimit.HasValue && coupon.UsageCount >= coupon.UsageLimit.Value)
            return Result<ValidateCouponResponse>.Success(new ValidateCouponResponse(false, "Coupon usage limit reached.", 0));

        if (coupon.MinimumOrderAmount.HasValue && subtotal < coupon.MinimumOrderAmount.Value)
            return Result<ValidateCouponResponse>.Success(new ValidateCouponResponse(false, $"Minimum order amount is {coupon.MinimumOrderAmount.Value}.", 0));

        decimal discountAmount = 0;
        if (coupon.DiscountType == DiscountType.FixedAmount)
        {
            discountAmount = coupon.DiscountValue;
        }
        else if (coupon.DiscountType == DiscountType.Percentage)
        {
            discountAmount = subtotal * (coupon.DiscountValue / 100);
            if (coupon.MaximumDiscountAmount.HasValue && discountAmount > coupon.MaximumDiscountAmount.Value)
            {
                discountAmount = coupon.MaximumDiscountAmount.Value;
            }
        }

        // Ensure discount doesn't exceed subtotal
        if (discountAmount > subtotal) discountAmount = subtotal;

        return Result<ValidateCouponResponse>.Success(new ValidateCouponResponse(true, null, discountAmount));
    }

    private static CouponDto MapToDto(Coupon c)
    {
        return new CouponDto(
            c.Id, c.Code, c.DiscountType, c.DiscountValue, c.MinimumOrderAmount,
            c.MaximumDiscountAmount, c.UsageLimit, c.UsageCount, c.StartDate, c.EndDate, c.IsActive
        );
    }
}
