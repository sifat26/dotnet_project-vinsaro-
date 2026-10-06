using Microsoft.EntityFrameworkCore;
using Vinsaro.Application.Common;
using Vinsaro.Application.Features.Checkout.DTOs;
using Vinsaro.Application.Features.Checkout.Interfaces;
using Vinsaro.Domain.Entities;
using Vinsaro.Infrastructure.Persistence;

namespace Vinsaro.Infrastructure.Checkout.Services;

public class CustomerAddressService : ICustomerAddressService
{
    private readonly VinsaroDbContext _dbContext;

    public CustomerAddressService(VinsaroDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<List<CustomerAddressDto>>> GetUserAddressesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var addresses = await _dbContext.CustomerAddresses
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.IsDefault)
            .ToListAsync(cancellationToken);

        return Result<List<CustomerAddressDto>>.Success(addresses.Select(MapToDto).ToList());
    }

    public async Task<Result<CustomerAddressDto>> GetAddressAsync(Guid userId, Guid addressId, CancellationToken cancellationToken = default)
    {
        var address = await _dbContext.CustomerAddresses
            .FirstOrDefaultAsync(a => a.Id == addressId && a.UserId == userId, cancellationToken);

        if (address == null) return Result<CustomerAddressDto>.Failure("Address not found.");

        return Result<CustomerAddressDto>.Success(MapToDto(address));
    }

    public async Task<Result<CustomerAddressDto>> CreateAddressAsync(Guid userId, CreateCustomerAddressRequest request, CancellationToken cancellationToken = default)
    {
        var hasAddresses = await _dbContext.CustomerAddresses.AnyAsync(a => a.UserId == userId, cancellationToken);
        var isDefault = request.IsDefault || !hasAddresses; // Force default if it's the first one

        if (isDefault)
        {
            await ClearDefaultsAsync(userId, cancellationToken);
        }

        var address = new CustomerAddress
        {
            UserId = userId,
            FullName = request.FullName,
            PhoneNumber = request.PhoneNumber,
            AddressLine1 = request.AddressLine1,
            AddressLine2 = request.AddressLine2,
            Area = request.Area,
            District = request.District,
            Division = request.Division,
            PostalCode = request.PostalCode,
            IsDefault = isDefault
        };

        _dbContext.CustomerAddresses.Add(address);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result<CustomerAddressDto>.Success(MapToDto(address));
    }

    public async Task<Result<CustomerAddressDto>> UpdateAddressAsync(Guid userId, Guid addressId, UpdateCustomerAddressRequest request, CancellationToken cancellationToken = default)
    {
        var address = await _dbContext.CustomerAddresses
            .FirstOrDefaultAsync(a => a.Id == addressId && a.UserId == userId, cancellationToken);

        if (address == null) return Result<CustomerAddressDto>.Failure("Address not found.");

        if (request.IsDefault && !address.IsDefault)
        {
            await ClearDefaultsAsync(userId, cancellationToken);
        }
        else if (!request.IsDefault && address.IsDefault)
        {
            // Preventing user from removing the default flag from the only default address directly without setting another.
            // Simplified: We allow it, but a checkout might complain if there is no default address.
        }

        address.FullName = request.FullName;
        address.PhoneNumber = request.PhoneNumber;
        address.AddressLine1 = request.AddressLine1;
        address.AddressLine2 = request.AddressLine2;
        address.Area = request.Area;
        address.District = request.District;
        address.Division = request.Division;
        address.PostalCode = request.PostalCode;
        address.IsDefault = request.IsDefault;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result<CustomerAddressDto>.Success(MapToDto(address));
    }

    public async Task<Result<bool>> DeleteAddressAsync(Guid userId, Guid addressId, CancellationToken cancellationToken = default)
    {
        var address = await _dbContext.CustomerAddresses
            .FirstOrDefaultAsync(a => a.Id == addressId && a.UserId == userId, cancellationToken);

        if (address == null) return Result<bool>.Failure("Address not found.");

        _dbContext.CustomerAddresses.Remove(address);
        await _dbContext.SaveChangesAsync(cancellationToken);

        // If we deleted the default, set another one as default if exists
        if (address.IsDefault)
        {
            var nextAddress = await _dbContext.CustomerAddresses.FirstOrDefaultAsync(a => a.UserId == userId, cancellationToken);
            if (nextAddress != null)
            {
                nextAddress.IsDefault = true;
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
        }

        return Result<bool>.Success(true);
    }

    public async Task<Result<CustomerAddressDto>> SetDefaultAddressAsync(Guid userId, Guid addressId, CancellationToken cancellationToken = default)
    {
        var address = await _dbContext.CustomerAddresses
            .FirstOrDefaultAsync(a => a.Id == addressId && a.UserId == userId, cancellationToken);

        if (address == null) return Result<CustomerAddressDto>.Failure("Address not found.");

        if (!address.IsDefault)
        {
            await ClearDefaultsAsync(userId, cancellationToken);
            address.IsDefault = true;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return Result<CustomerAddressDto>.Success(MapToDto(address));
    }

    private async Task ClearDefaultsAsync(Guid userId, CancellationToken cancellationToken)
    {
        var defaults = await _dbContext.CustomerAddresses
            .Where(a => a.UserId == userId && a.IsDefault)
            .ToListAsync(cancellationToken);

        foreach (var def in defaults)
        {
            def.IsDefault = false;
        }
    }

    private static CustomerAddressDto MapToDto(CustomerAddress a)
    {
        return new CustomerAddressDto(
            a.Id, a.FullName, a.PhoneNumber, a.AddressLine1, a.AddressLine2,
            a.Area, a.District, a.Division, a.PostalCode, a.IsDefault
        );
    }
}
