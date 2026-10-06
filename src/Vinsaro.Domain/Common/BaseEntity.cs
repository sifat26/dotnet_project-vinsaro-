namespace Vinsaro.Domain.Common;

/// <summary>
/// Base class for all domain entities. Provides Id, CreatedAt and UpdatedAt audit fields.
/// All timestamps are stored and surfaced as UTC.
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
