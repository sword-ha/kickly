using SportsBooking.Domain.Enums;

namespace SportsBooking.Application.DTOs;

public sealed record RegisterRequest(string FirstName, string LastName, string Email, string PhoneNumber, string Password);

public sealed record LoginRequest(string Email, string Password);

public sealed record AuthResponse(
    int UserId,
    string FirstName,
    string LastName,
    string Email,
    UserRole Role,
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiresAtUtc,
    DateTime RefreshTokenExpiresAtUtc);

public sealed record RegisterResponse(int UserId, string Email, string Message);

public sealed record MessageResponse(string Message);

public sealed record ConfirmEmailRequest(int UserId, string Token);

public sealed record ResendConfirmationRequest(string Email);

public sealed record ForgotPasswordRequest(string Email);

public sealed record ResetPasswordRequest(int UserId, string Token, string NewPassword);

public sealed record ChangePasswordRequest(string CurrentPassword, string NewPassword);

public sealed record RefreshTokenRequest(string RefreshToken);

public sealed record UserProfileDto(
    int Id,
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber,
    UserRole Role,
    decimal? Latitude,
    decimal? Longitude);

public sealed record UpdateProfileRequest(string FirstName, string LastName, string PhoneNumber);

public sealed record UpdateLocationRequest(decimal Latitude, decimal Longitude);
