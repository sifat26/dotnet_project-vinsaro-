using Microsoft.EntityFrameworkCore;
using Vinsaro.Application.Common;
using Vinsaro.Application.Features.Payments.DTOs;
using Vinsaro.Application.Features.Payments.Interfaces;
using Vinsaro.Domain.Entities;
using Vinsaro.Domain.Enums;
using Vinsaro.Infrastructure.Persistence;

namespace Vinsaro.Infrastructure.Payments.Services;

public class PaymentService : IPaymentService
{
    private readonly VinsaroDbContext _dbContext;

    public PaymentService(VinsaroDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<PaymentDto>> ProcessPaymentAsync(Guid userId, ProcessPaymentRequest request, CancellationToken cancellationToken = default)
    {
        var order = await _dbContext.Orders
            .FirstOrDefaultAsync(o => o.Id == request.OrderId && o.UserId == userId, cancellationToken);

        if (order == null) return Result<PaymentDto>.Failure("Order not found.");

        if (order.PaymentStatus == PaymentStatus.Paid)
            return Result<PaymentDto>.Failure("Order is already paid.");

        var payment = new Payment
        {
            OrderId = order.Id,
            PaymentMethod = request.PaymentMethod,
            Amount = order.TotalAmount,
            Currency = order.Currency,
            ProviderTransactionId = request.ProviderTransactionId
        };

        if (request.PaymentMethod == PaymentMethod.CashOnDelivery)
        {
            payment.PaymentStatus = PaymentStatus.Pending; // Will be paid when delivered
            order.PaymentStatus = PaymentStatus.Pending;
        }
        else
        {
            // Simulate digital payment processing logic
            payment.PaymentStatus = PaymentStatus.Paid;
            payment.PaidAt = DateTime.UtcNow;
            
            order.PaymentStatus = PaymentStatus.Paid;
        }

        _dbContext.Payments.Add(payment);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result<PaymentDto>.Success(MapToDto(payment));
    }

    public async Task<Result<PaymentDto>> UpdatePaymentStatusAsync(Guid paymentId, UpdatePaymentStatusRequest request, CancellationToken cancellationToken = default)
    {
        var payment = await _dbContext.Payments
            .Include(p => p.Order)
            .FirstOrDefaultAsync(p => p.Id == paymentId, cancellationToken);

        if (payment == null) return Result<PaymentDto>.Failure("Payment not found.");

        payment.PaymentStatus = request.PaymentStatus;
        if (request.ProviderTransactionId != null)
            payment.ProviderTransactionId = request.ProviderTransactionId;

        if (request.PaymentStatus == PaymentStatus.Paid && payment.PaidAt == null)
        {
            payment.PaidAt = DateTime.UtcNow;
            payment.Order.PaymentStatus = PaymentStatus.Paid;
        }
        else if (request.PaymentStatus == PaymentStatus.Failed)
        {
            payment.Order.PaymentStatus = PaymentStatus.Failed;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result<PaymentDto>.Success(MapToDto(payment));
    }

    public async Task<Result<List<PaymentDto>>> GetOrderPaymentsAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var payments = await _dbContext.Payments
            .Where(p => p.OrderId == orderId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);

        return Result<List<PaymentDto>>.Success(payments.Select(MapToDto).ToList());
    }

    private static PaymentDto MapToDto(Payment p)
    {
        return new PaymentDto(
            p.Id, p.OrderId, p.PaymentMethod, p.PaymentStatus, p.Amount, p.Currency,
            p.ProviderTransactionId, p.ProviderReference, p.PaidAt
        );
    }
}
