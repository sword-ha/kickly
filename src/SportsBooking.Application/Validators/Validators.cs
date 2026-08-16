using FluentValidation;
using SportsBooking.Application.DTOs;

namespace SportsBooking.Application.Validators;

public sealed class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(255);
        RuleFor(x => x.PhoneNumber).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6).MaximumLength(100);
    }
}

public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}

public sealed class UpdateProfileRequestValidator : AbstractValidator<UpdateProfileRequest>
{
    public UpdateProfileRequestValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.PhoneNumber).NotEmpty().MaximumLength(20);
    }
}

public sealed class UpdateLocationRequestValidator : AbstractValidator<UpdateLocationRequest>
{
    public UpdateLocationRequestValidator()
    {
        RuleFor(x => x.Latitude).InclusiveBetween(-90m, 90m);
        RuleFor(x => x.Longitude).InclusiveBetween(-180m, 180m);
    }
}

public sealed class CreateBookingRequestValidator : AbstractValidator<CreateBookingRequest>
{
    public CreateBookingRequestValidator()
    {
        RuleFor(x => x.FieldId).GreaterThan(0);
        RuleFor(x => x.DurationHours).InclusiveBetween(1, 4);
        RuleFor(x => x.Date).Must(d => d >= DateOnly.FromDateTime(DateTime.UtcNow.Date))
            .WithMessage("Booking date cannot be in the past.");
    }
}

public sealed class CreateReviewRequestValidator : AbstractValidator<CreateReviewRequest>
{
    public CreateReviewRequestValidator()
    {
        RuleFor(x => x.BookingId).GreaterThan(0);
        RuleFor(x => x.Rating).InclusiveBetween(1, 5);
        RuleFor(x => x.Comment).MaximumLength(1000);
    }
}

public sealed class ForgotPasswordRequestValidator : AbstractValidator<ForgotPasswordRequest>
{
    public ForgotPasswordRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(255);
    }
}

public sealed class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
{
    public ResetPasswordRequestValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.Token).NotEmpty();
        RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(6).MaximumLength(100);
    }
}

public sealed class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordRequestValidator()
    {
        RuleFor(x => x.CurrentPassword).NotEmpty();
        RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(6).MaximumLength(100);
    }
}

public sealed class ConfirmEmailRequestValidator : AbstractValidator<ConfirmEmailRequest>
{
    public ConfirmEmailRequestValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.Token).NotEmpty();
    }
}

public sealed class ResendConfirmationRequestValidator : AbstractValidator<ResendConfirmationRequest>
{
    public ResendConfirmationRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(255);
    }
}

public sealed class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequest>
{
    public RefreshTokenRequestValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty();
    }
}

public sealed class CreatePaymentRequestValidator : AbstractValidator<CreatePaymentRequest>
{
    public CreatePaymentRequestValidator()
    {
        RuleFor(x => x.BookingId).GreaterThan(0);
        RuleFor(x => x.Method).IsInEnum();
    }
}

public sealed class RefundPaymentRequestValidator : AbstractValidator<RefundPaymentRequest>
{
    public RefundPaymentRequestValidator()
    {
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}

public sealed class CreateSportRequestValidator : AbstractValidator<CreateSportRequest>
{
    public CreateSportRequestValidator()
    {
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(100)
            .Matches("^[a-z0-9-]+$").WithMessage("Slug may only contain lowercase letters, digits and dashes.");
        RuleFor(x => x.Description).MaximumLength(1000);
    }
}

public sealed class UpdateSportRequestValidator : AbstractValidator<UpdateSportRequest>
{
    public UpdateSportRequestValidator()
    {
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(100)
            .Matches("^[a-z0-9-]+$").WithMessage("Slug may only contain lowercase letters, digits and dashes.");
        RuleFor(x => x.Description).MaximumLength(1000);
    }
}

public sealed class CreateFacilityRequestValidator : AbstractValidator<CreateFacilityRequest>
{
    public CreateFacilityRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Icon).MaximumLength(50);
    }
}

public sealed class UpdateFacilityRequestValidator : AbstractValidator<UpdateFacilityRequest>
{
    public UpdateFacilityRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Icon).MaximumLength(50);
    }
}

public sealed class CreateFieldRequestValidator : AbstractValidator<CreateFieldRequest>
{
    public CreateFieldRequestValidator()
    {
        RuleFor(x => x.SportId).GreaterThan(0);
        RuleFor(x => x.LocationId).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.Address).NotEmpty().MaximumLength(500);
        RuleFor(x => x.City).NotEmpty().MaximumLength(100);
        RuleFor(x => x.FieldType).IsInEnum();
        RuleFor(x => x.Latitude).InclusiveBetween(-90m, 90m);
        RuleFor(x => x.Longitude).InclusiveBetween(-180m, 180m);
        RuleFor(x => x.DayPricePerHour).GreaterThan(0);
        RuleFor(x => x.NightPricePerHour).GreaterThan(0);
    }
}

public sealed class UpdateFieldRequestValidator : AbstractValidator<UpdateFieldRequest>
{
    public UpdateFieldRequestValidator()
    {
        RuleFor(x => x.SportId).GreaterThan(0);
        RuleFor(x => x.LocationId).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.Address).NotEmpty().MaximumLength(500);
        RuleFor(x => x.City).NotEmpty().MaximumLength(100);
        RuleFor(x => x.FieldType).IsInEnum();
        RuleFor(x => x.Latitude).InclusiveBetween(-90m, 90m);
        RuleFor(x => x.Longitude).InclusiveBetween(-180m, 180m);
        RuleFor(x => x.DayPricePerHour).GreaterThan(0);
        RuleFor(x => x.NightPricePerHour).GreaterThan(0);
    }
}

public sealed class SetAvailabilityRequestValidator : AbstractValidator<SetAvailabilityRequest>
{
    public SetAvailabilityRequestValidator()
    {
        RuleFor(x => x.Date).Must(d => d >= DateOnly.FromDateTime(DateTime.UtcNow.Date))
            .WithMessage("Availability date cannot be in the past.");
    }
}

public sealed class UpdateAvailabilityRequestValidator : AbstractValidator<UpdateAvailabilityRequest>
{
    public UpdateAvailabilityRequestValidator()
    {
        RuleFor(x => x.CloseTime).Must((request, close) => close > request.OpenTime)
            .WithMessage("Close time must be after open time.")
            .When(x => !x.IsClosed);
    }
}

public sealed class UpdateReviewRequestValidator : AbstractValidator<UpdateReviewRequest>
{
    public UpdateReviewRequestValidator()
    {
        RuleFor(x => x.Rating).InclusiveBetween(1, 5);
        RuleFor(x => x.Comment).MaximumLength(1000);
    }
}

public sealed class UpdateOwnerBookingStatusRequestValidator : AbstractValidator<UpdateOwnerBookingStatusRequest>
{
    public UpdateOwnerBookingStatusRequestValidator()
    {
        RuleFor(x => x.Status).IsInEnum();
        RuleFor(x => x.Reason).MaximumLength(500);
    }
}

public sealed class BroadcastNotificationRequestValidator : AbstractValidator<BroadcastNotificationRequest>
{
    public BroadcastNotificationRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Message).NotEmpty().MaximumLength(2000);
    }
}

public sealed class CreateLocationRequestValidator : AbstractValidator<CreateLocationRequest>
{
    public CreateLocationRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.City).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Governorate).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Address).MaximumLength(500);
        RuleFor(x => x.Latitude).InclusiveBetween(-90m, 90m);
        RuleFor(x => x.Longitude).InclusiveBetween(-180m, 180m);
    }
}

public sealed class AdminUpdateLocationRequestValidator : AbstractValidator<AdminUpdateLocationRequest>
{
    public AdminUpdateLocationRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.City).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Governorate).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Address).MaximumLength(500);
        RuleFor(x => x.Latitude).InclusiveBetween(-90m, 90m);
        RuleFor(x => x.Longitude).InclusiveBetween(-180m, 180m);
    }
}