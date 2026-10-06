using Microsoft.AspNetCore.Identity;
using Vinsaro.Domain.Entities;

namespace Vinsaro.Infrastructure.Identity;

/// <summary>
/// Custom application user extending ASP.NET Core Identity's IdentityUser.
/// Password is managed by Identity's secure hashing — never stored in plaintext.
/// Lives in Infrastructure because it depends on Microsoft.AspNetCore.Identity.
/// </summary>
public class ApplicationUser : IdentityUser<Guid>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<CustomerAddress> Addresses { get; set; } = [];
    public Cart? Cart { get; set; }
    public ICollection<Order> Orders { get; set; } = [];
    public ICollection<ProductReview> Reviews { get; set; } = [];
    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
}

/// <summary>
/// Custom role entity extending IdentityRole with a Guid primary key.
/// </summary>
public class ApplicationRole : IdentityRole<Guid>
{
    public ApplicationRole() { }
    public ApplicationRole(string roleName) : base(roleName) { }
}
