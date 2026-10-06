using Vinsaro.Domain.Common;

namespace Vinsaro.Infrastructure.Identity;

/// <summary>
/// Secure refresh token stored server-side. Only the hashed value is persisted — the raw token
/// is returned to the client exactly once and cannot be recovered from the database.
/// Supports rotation (each use issues a new token) and explicit revocation.
/// </summary>
public class RefreshToken : BaseEntity
{
    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;

    /// <summary>SHA-256 hash of the token value.</summary>
    public string TokenHash { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; } = false;
    public DateTime? RevokedAt { get; set; }

    /// <summary>Hash of the token that replaced this one (for audit trail on rotation).</summary>
    public string? ReplacedByTokenHash { get; set; }

    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsActive => !IsRevoked && !IsExpired;
}
