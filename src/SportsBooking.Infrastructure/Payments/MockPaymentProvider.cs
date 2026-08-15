using Microsoft.Extensions.Options;
using SportsBooking.Application.Interfaces;
using SportsBooking.Application.Options;

namespace SportsBooking.Infrastructure.Payments;

public sealed class MockPaymentProvider : IPaymentProvider
{
    private readonly PaymentOptions _options;

    public MockPaymentProvider(IOptions<PaymentOptions> options)
    {
        _options = options.Value;
    }

    public Task<PaymentChargeResult> ChargeAsync(PaymentChargeRequest request, CancellationToken ct = default)
    {
        // Simulate a short processing delay.
        if (ct.IsCancellationRequested)
        {
            return Task.FromCanceled<PaymentChargeResult>(ct);
        }

        if (!_options.Mock.AlwaysSucceed)
        {
            var reason = _options.Mock.SimulatedFailureReason ?? "The mock payment provider declined the transaction.";
            return Task.FromResult(new PaymentChargeResult(false, null, reason));
        }

        var transactionId = $"MOCK-{Guid.NewGuid():N}";
        return Task.FromResult(new PaymentChargeResult(true, transactionId, null));
    }

    public Task<PaymentRefundResult> RefundAsync(PaymentRefundRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.TransactionId))
        {
            return Task.FromResult(new PaymentRefundResult(false, "Missing transaction id."));
        }

        return Task.FromResult(new PaymentRefundResult(true, null));
    }
}
