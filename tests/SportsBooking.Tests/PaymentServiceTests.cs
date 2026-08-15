using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using SportsBooking.Application.DTOs;
using SportsBooking.Application.Interfaces;
using SportsBooking.Application.Options;
using SportsBooking.Application.Services;
using SportsBooking.Domain.Entities;
using SportsBooking.Domain.Enums;
using SportsBooking.Domain.Exceptions;
using Xunit;

namespace SportsBooking.Tests;

public sealed class PaymentServiceTests
{
    private readonly Mock<IPaymentRepository> _paymentRepo;
    private readonly Mock<IBookingRepository> _bookingRepo;
    private readonly Mock<IPaymentProvider> _provider;
    private readonly Mock<INotificationService> _notificationService;
    private readonly PaymentService _service;

    public PaymentServiceTests()
    {
        _paymentRepo = new Mock<IPaymentRepository>();
        _bookingRepo = new Mock<IBookingRepository>();
        _provider = new Mock<IPaymentProvider>();
        _notificationService = new Mock<INotificationService>();
        var options = Options.Create(new PaymentOptions
        {
            Provider = "Mock",
            Currency = "EGP",
            Mock = new MockPaymentOptions { AlwaysSucceed = true }
        });

        _service = new PaymentService(_paymentRepo.Object, _bookingRepo.Object, _provider.Object, _notificationService.Object, options);
    }

    private static Booking CreateBooking(int userId = 1, int bookingId = 10, BookingStatus status = BookingStatus.PendingPayment, decimal price = 150m)
        => new()
        {
            Id = bookingId,
            UserId = userId,
            FieldId = 1,
            TotalPrice = price,
            Status = status,
            Field = new Field { Id = 1, IsActive = true },
            User = new User { Id = userId }
        };

    private static Payment CreatePayment(int bookingId = 10, PaymentStatus status = PaymentStatus.Paid)
        => new()
        {
            Id = 1,
            BookingId = bookingId,
            Amount = 150m,
            Method = PaymentMethod.Card,
            Status = status,
            Provider = "Mock",
            TransactionId = "MOCK-TXN",
            Booking = CreateBooking()
        };

    [Fact]
    public async Task Create_BookingNotFound_ThrowsNotFound()
    {
        _bookingRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Booking?)null);

        var act = async () => await _service.CreateAsync(1, new CreatePaymentRequest(999, PaymentMethod.Card), CancellationToken.None);
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Create_NotOwnersBooking_ThrowsForbidden()
    {
        _bookingRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateBooking(userId: 2));

        var act = async () => await _service.CreateAsync(1, new CreatePaymentRequest(10, PaymentMethod.Card), CancellationToken.None);
        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task Create_BookingNotAwaitingPayment_ThrowsConflict()
    {
        _bookingRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateBooking(status: BookingStatus.Confirmed));

        var act = async () => await _service.CreateAsync(1, new CreatePaymentRequest(10, PaymentMethod.Card), CancellationToken.None);
        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Create_PaymentAlreadyProcessing_ThrowsConflict()
    {
        _bookingRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateBooking());
        _paymentRepo.Setup(r => r.GetLatestByBookingIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreatePayment(status: PaymentStatus.Pending));
        _paymentRepo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var act = async () => await _service.CreateAsync(1, new CreatePaymentRequest(10, PaymentMethod.Card), CancellationToken.None);
        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Create_SuccessfulPayment_ConfirmsBookingAndSavesPayment()
    {
        _bookingRepo.Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateBooking());
        _paymentRepo.Setup(r => r.GetLatestByBookingIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Payment?)null);
        _provider.Setup(p => p.ChargeAsync(It.IsAny<PaymentChargeRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentChargeResult(true, "MOCK-ABC123", null));
        _paymentRepo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await _service.CreateAsync(1, new CreatePaymentRequest(10, PaymentMethod.Card), CancellationToken.None);

        result.Status.Should().Be(PaymentStatus.Paid);
        result.Amount.Should().Be(150m);
        result.TransactionId.Should().Be("MOCK-ABC123");
        result.PaidAtUtc.Should().NotBeNull();
        _paymentRepo.Verify(r => r.AddAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Create_FailedPayment_ThrowsPaymentFailedAndStoresFailure()
    {
        _bookingRepo.Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateBooking());
        _paymentRepo.Setup(r => r.GetLatestByBookingIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Payment?)null);
        _provider.Setup(p => p.ChargeAsync(It.IsAny<PaymentChargeRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentChargeResult(false, null, "Insufficient funds."));
        _paymentRepo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var act = async () => await _service.CreateAsync(1, new CreatePaymentRequest(10, PaymentMethod.Card), CancellationToken.None);
        var ex = await act.Should().ThrowAsync<PaymentFailedException>();
        ex.Which.Message.Should().Contain("Insufficient funds");

        _paymentRepo.Verify(r => r.AddAsync(It.Is<Payment>(p => p.Status == PaymentStatus.Failed), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetById_AnotherUsersPayment_ThrowsForbidden()
    {
        var payment = CreatePayment();
        payment.Booking.UserId = 2;
        _paymentRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);

        var act = async () => await _service.GetByIdAsync(1, 1, CancellationToken.None);
        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task GetStatus_OwnPayment_ReturnsStatus()
    {
        _paymentRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreatePayment());

        var result = await _service.GetStatusAsync(1, 1, CancellationToken.None);

        result.PaymentId.Should().Be(1);
        result.Status.Should().Be(PaymentStatus.Paid);
    }

    [Fact]
    public async Task Webhook_Paid_ConfirmsBooking()
    {
        var payment = CreatePayment(status: PaymentStatus.Pending);
        payment.Booking.Status = BookingStatus.PendingPayment;
        _paymentRepo.Setup(r => r.GetByTransactionIdAsync("MOCK-TXN", It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);
        _paymentRepo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await _service.ProcessWebhookAsync(
            new PaymentWebhookRequest("MOCK-TXN", PaymentStatus.Paid, "Mock"), CancellationToken.None);

        result.Status.Should().Be(PaymentStatus.Paid);
        payment.Booking.Status.Should().Be(BookingStatus.Confirmed);
    }

    [Fact]
    public async Task Webhook_UnknownTransaction_ThrowsNotFound()
    {
        _paymentRepo.Setup(r => r.GetByTransactionIdAsync("UNKNOWN", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Payment?)null);

        var act = async () => await _service.ProcessWebhookAsync(
            new PaymentWebhookRequest("UNKNOWN", PaymentStatus.Paid), CancellationToken.None);
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Refund_PaidPayment_Refunds()
    {
        _paymentRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreatePayment());
        _provider.Setup(p => p.RefundAsync(It.IsAny<PaymentRefundRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentRefundResult(true, null));
        _paymentRepo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await _service.RefundAsync(1, 1, new RefundPaymentRequest("Changed my mind"), CancellationToken.None);

        result.Status.Should().Be(PaymentStatus.Refunded);
        result.RefundedAtUtc.Should().NotBeNull();
    }

    [Fact]
    public async Task Refund_NotPaid_ThrowsConflict()
    {
        _paymentRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreatePayment(status: PaymentStatus.Pending));

        var act = async () => await _service.RefundAsync(1, 1, new RefundPaymentRequest("reason"), CancellationToken.None);
        await act.Should().ThrowAsync<ConflictException>();
    }
}
