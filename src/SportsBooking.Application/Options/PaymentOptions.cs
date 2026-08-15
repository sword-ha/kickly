namespace SportsBooking.Application.Options;

public sealed class PaymentOptions
{
    public const string SectionName = "Payment";

    public string Provider { get; set; } = "Mock";
    public string Currency { get; set; } = "EGP";
    public MockPaymentOptions Mock { get; set; } = new();
}

public sealed class MockPaymentOptions
{
    public bool AlwaysSucceed { get; set; } = true;
    public string? SimulatedFailureReason { get; set; }
}
