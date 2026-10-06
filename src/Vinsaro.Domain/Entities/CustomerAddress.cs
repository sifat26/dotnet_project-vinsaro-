using Vinsaro.Domain.Common;

namespace Vinsaro.Domain.Entities;

/// <summary>
/// A customer's saved shipping address. Customers may have multiple addresses; one can be default.
/// Address fields are designed for the Bangladeshi address system.
/// </summary>
public class CustomerAddress : BaseEntity
{
    public Guid UserId { get; set; }

    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string AddressLine1 { get; set; } = string.Empty;
    public string? AddressLine2 { get; set; }
    public string Area { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string Division { get; set; } = string.Empty;
    public string? PostalCode { get; set; }
    public bool IsDefault { get; set; } = false;
}
