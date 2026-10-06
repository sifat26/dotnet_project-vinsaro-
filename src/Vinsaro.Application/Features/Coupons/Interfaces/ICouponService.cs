using Vinsaro.Application.Common;
using Vinsaro.Application.Features.Coupons.DTOs;

namespace Vinsaro.Application.Features.Coupons.Interfaces;

public interface ICouponService
{
    Task<Result<CouponDto>> CreateCouponAsync(CreateCouponRequest request, CancellationToken cancellationToken = default);
    Task<Result<bool>> DeleteCouponAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<CouponDto>> GetCouponByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<Result<List<CouponDto>>> GetAllCouponsAsync(CancellationToken cancellationToken = default);
    Task<Result<ValidateCouponResponse>> ValidateAndCalculateDiscountAsync(string code, decimal subtotal, CancellationToken cancellationToken = default);
}
