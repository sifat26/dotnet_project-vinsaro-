using Vinsaro.Application.Common;
using Vinsaro.Application.Features.Checkout.DTOs;

namespace Vinsaro.Application.Features.Checkout.Interfaces;

public interface ICustomerAddressService
{
    Task<Result<List<CustomerAddressDto>>> GetUserAddressesAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Result<CustomerAddressDto>> GetAddressAsync(Guid userId, Guid addressId, CancellationToken cancellationToken = default);
    Task<Result<CustomerAddressDto>> CreateAddressAsync(Guid userId, CreateCustomerAddressRequest request, CancellationToken cancellationToken = default);
    Task<Result<CustomerAddressDto>> UpdateAddressAsync(Guid userId, Guid addressId, UpdateCustomerAddressRequest request, CancellationToken cancellationToken = default);
    Task<Result<bool>> DeleteAddressAsync(Guid userId, Guid addressId, CancellationToken cancellationToken = default);
    Task<Result<CustomerAddressDto>> SetDefaultAddressAsync(Guid userId, Guid addressId, CancellationToken cancellationToken = default);
}
