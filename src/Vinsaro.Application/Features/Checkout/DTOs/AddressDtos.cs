namespace Vinsaro.Application.Features.Checkout.DTOs;

public record CustomerAddressDto(
    Guid Id,
    string FullName,
    string PhoneNumber,
    string AddressLine1,
    string? AddressLine2,
    string Area,
    string District,
    string Division,
    string? PostalCode,
    bool IsDefault
);

public record CreateCustomerAddressRequest(
    string FullName,
    string PhoneNumber,
    string AddressLine1,
    string? AddressLine2,
    string Area,
    string District,
    string Division,
    string? PostalCode,
    bool IsDefault
);

public record UpdateCustomerAddressRequest(
    string FullName,
    string PhoneNumber,
    string AddressLine1,
    string? AddressLine2,
    string Area,
    string District,
    string Division,
    string? PostalCode,
    bool IsDefault
);
