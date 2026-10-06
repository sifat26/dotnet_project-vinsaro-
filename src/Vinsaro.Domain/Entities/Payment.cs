using Vinsaro.Domain.Common;
using Vinsaro.Domain.Enums;

namespace Vinsaro.Domain.Entities;

/// <summary>
/// Payment record for an order. Multiple payment attempts may exist (e.g. failed then successful).
/// Payment status is set server-side only — the client cannot promote payment status.
/// </summary>
public class Payment : BaseEntity
{
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;

    public PaymentMethod PaymentMethod { get; set; }
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

    /// <summary>Amount in BDT (NUMERIC(18,2)).</summary>
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "BDT";

    // Gateway fields — populated when a real or simulated payment gateway is used
    public string? ProviderTransactionId { get; set; }
    public string? ProviderReference { get; set; }

    public DateTime? PaidAt { get; set; }
}
