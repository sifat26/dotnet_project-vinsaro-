namespace Vinsaro.Domain.Enums;

public enum OrderStatus
{
    Pending = 0,
    Confirmed = 1,
    Processing = 2,
    Shipped = 3,
    Delivered = 4,
    Cancelled = 5
}

public enum PaymentStatus
{
    Unpaid = 0,
    Pending = 1,
    Paid = 2,
    Failed = 3,
    Refunded = 4
}

public enum PaymentMethod
{
    CashOnDelivery = 0,
    SimulatedCard = 1   // Development-only simulated payment
}

public enum DiscountType
{
    Percentage = 0,
    FixedAmount = 1
}

public enum MovementType
{
    Purchase = 0,       // Stock deducted when order is placed
    Cancellation = 1,   // Stock restored when order is cancelled
    Return = 2,         // Stock restored when item is returned
    ManualAdjustment = 3 // Admin manual stock correction
}
