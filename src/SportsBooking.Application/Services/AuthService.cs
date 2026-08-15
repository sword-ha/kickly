using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using SportsBooking.Application.DTOs;
using SportsBooking.Application.Interfaces;
using SportsBooking.Application.Options;
using SportsBooking.Domain.Entities;
using SportsBooking.Domain.Enums;
using SportsBooking.Domain.Exceptions;

namespace SportsBooking.Application.Services;

public sealed class AuthService : IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole<int>> _roleManager;
    private readonly ITokenService _tokenService;
    private readonly IEmailSender _emailSender;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly JwtOptions _jwtOptions;
    private readonly AppOptions _appOptions;

    public AuthService(
        UserManager<User> userManager,
        RoleManager<IdentityRole<int>> roleManager,
        ITokenService tokenService,
        IEmailSender emailSender,
        IRefreshTokenRepository refreshTokenRepository,
        IOptions<JwtOptions> jwtOptions,
        IOptions<AppOptions> appOptions)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _tokenService = tokenService;
        _emailSender = emailSender;
        _refreshTokenRepository = refreshTokenRepository;
        _jwtOptions = jwtOptions.Value;
        _appOptions = appOptions.Value;
    }

    public async Task<RegisterResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        if (await _userManager.FindByEmailAsync(normalizedEmail) is not null)
        {
            throw new ConflictException("An account with this email already exists.");
        }

        var user = new User
        {
            UserName = normalizedEmail,
            Email = normalizedEmail,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            PhoneNumber = request.PhoneNumber.Trim(),
            Role = UserRole.Customer,
            IsActive = true
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            throw new ValidationDomainException(FormatErrors(result.Errors));
        }

        await EnsureRoleAsync(UserRole.Customer.ToString());
        await _userManager.AddToRoleAsync(user, UserRole.Customer.ToString());

        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        await _emailSender.SendAsync(
            user.Email!,
            "Confirm your email - Sports Booking",
            BuildConfirmationEmail(user.Id, token),
            ct);

        return new RegisterResponse(
            user.Id,
            user.Email!,
            "Registration successful. Please check your email to confirm your account before logging in.");
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email.Trim().ToLowerInvariant())
            ?? throw new ValidationDomainException("Invalid email or password.");

        if (!user.IsActive)
        {
            throw new ForbiddenException("This account has been deactivated.");
        }

        if (!user.EmailConfirmed)
        {
            throw new EmailNotConfirmedException("Email is not confirmed. Please confirm your email before logging in.");
        }

        if (await _userManager.IsLockedOutAsync(user))
        {
            throw new ForbiddenException("Account is locked due to multiple failed attempts. Please try again later.");
        }

        if (!await _userManager.CheckPasswordAsync(user, request.Password))
        {
            await _userManager.AccessFailedAsync(user);
            throw new ValidationDomainException("Invalid email or password.");
        }

        await _userManager.ResetAccessFailedCountAsync(user);
        return await BuildAuthResponseAsync(user, ct);
    }

    public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken ct = default)
    {
        var tokenHash = _tokenService.HashRefreshToken(request.RefreshToken);
        var storedToken = await _refreshTokenRepository.GetByTokenHashAsync(tokenHash, ct)
            ?? throw new UnauthorizedAccessException("Invalid refresh token.");

        if (storedToken.IsRevoked)
        {
            throw new UnauthorizedAccessException("Refresh token has been revoked.");
        }

        if (storedToken.IsExpired)
        {
            throw new UnauthorizedAccessException("Refresh token has expired. Please log in again.");
        }

        var user = storedToken.User;
        if (user is null || !user.IsActive)
        {
            throw new UnauthorizedAccessException("User account is no longer active.");
        }

        storedToken.RevokedAtUtc = DateTime.UtcNow;
        await _refreshTokenRepository.SaveChangesAsync(ct);

        return await BuildAuthResponseAsync(user, ct);
    }

    public async Task RevokeTokenAsync(string refreshToken, CancellationToken ct = default)
    {
        var tokenHash = _tokenService.HashRefreshToken(refreshToken);
        var storedToken = await _refreshTokenRepository.GetByTokenHashAsync(tokenHash, ct);
        if (storedToken is null || storedToken.IsRevoked)
        {
            return;
        }

        storedToken.RevokedAtUtc = DateTime.UtcNow;
        await _refreshTokenRepository.SaveChangesAsync(ct);
    }

    public async Task<MessageResponse> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken ct = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email.Trim().ToLowerInvariant());

        // Always return the same message to avoid leaking which emails exist.
        if (user is not null && user.EmailConfirmed)
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            await _emailSender.SendAsync(
                user.Email!,
                "Reset your password - Sports Booking",
                BuildPasswordResetEmail(user.Id, token),
                ct);
        }

        return new MessageResponse("If the email exists and is confirmed, a password reset link has been sent.");
    }

    public async Task<MessageResponse> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(request.UserId.ToString())
            ?? throw new ValidationDomainException("Invalid password reset request.");

        var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
        if (!result.Succeeded)
        {
            throw new ValidationDomainException(FormatErrors(result.Errors));
        }

        return new MessageResponse("Your password has been reset. You can now log in.");
    }

    public async Task<MessageResponse> ChangePasswordAsync(int userId, ChangePasswordRequest request, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString())
            ?? throw new NotFoundException("User was not found.");

        var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!result.Succeeded)
        {
            throw new ValidationDomainException(FormatErrors(result.Errors));
        }

        var activeTokens = await _refreshTokenRepository.GetActiveByUserIdAsync(userId, ct);
        foreach (var token in activeTokens)
        {
            token.RevokedAtUtc = DateTime.UtcNow;
        }

        await _refreshTokenRepository.SaveChangesAsync(ct);
        return new MessageResponse("Password changed successfully.");
    }

    public async Task<MessageResponse> ConfirmEmailAsync(ConfirmEmailRequest request, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(request.UserId.ToString())
            ?? throw new NotFoundException("User was not found.");

        var result = await _userManager.ConfirmEmailAsync(user, request.Token);
        if (!result.Succeeded)
        {
            throw new ValidationDomainException("Invalid or expired email confirmation token.");
        }

        return new MessageResponse("Email confirmed successfully. You can now log in.");
    }

    public async Task<MessageResponse> ResendConfirmationAsync(ResendConfirmationRequest request, CancellationToken ct = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email.Trim().ToLowerInvariant());

        if (user is not null && !user.EmailConfirmed)
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            await _emailSender.SendAsync(
                user.Email!,
                "Confirm your email - Sports Booking",
                BuildConfirmationEmail(user.Id, token),
                ct);
        }

        return new MessageResponse("If the email exists and is not yet confirmed, a new confirmation link has been sent.");
    }

    private async Task<AuthResponse> BuildAuthResponseAsync(User user, CancellationToken ct)
    {
        var accessToken = _tokenService.CreateToken(user.Id, user.Email ?? string.Empty, user.Role);
        var refreshToken = _tokenService.CreateRefreshToken();
        var refreshTokenExpiresAt = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenExpiryDays);

        await _refreshTokenRepository.AddAsync(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = _tokenService.HashRefreshToken(refreshToken),
            ExpiresAtUtc = refreshTokenExpiresAt
        }, ct);
        await _refreshTokenRepository.SaveChangesAsync(ct);

        return new AuthResponse(
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email ?? string.Empty,
            user.Role,
            accessToken,
            refreshToken,
            DateTime.UtcNow.AddMinutes(_jwtOptions.AccessTokenMinutes),
            refreshTokenExpiresAt);
    }

    private async Task EnsureRoleAsync(string roleName)
    {
        if (!await _roleManager.RoleExistsAsync(roleName))
        {
            await _roleManager.CreateAsync(new IdentityRole<int>(roleName));
        }
    }

    private string BuildConfirmationEmail(int userId, string token)
    {
        var url = $"{_appOptions.ClientBaseUrl}{_appOptions.ConfirmEmailPath}?userId={userId}&token={Uri.EscapeDataString(token)}";
        return $"""
            <h2>Welcome to Sports Booking!</h2>
            <p>Thanks for registering. Please confirm your email address by clicking the link below:</p>
            <p><a href="{url}">Confirm my email</a></p>
            <p>If the button does not work, copy this link into your browser:</p>
            <p><small>{url}</small></p>
            """;
    }

    private string BuildPasswordResetEmail(int userId, string token)
    {
        var url = $"{_appOptions.ClientBaseUrl}{_appOptions.ResetPasswordPath}?userId={userId}&token={Uri.EscapeDataString(token)}";
        return $"""
            <h2>Reset your password</h2>
            <p>You requested to reset your password. Click the link below to choose a new one:</p>
            <p><a href="{url}">Reset my password</a></p>
            <p>If the button does not work, copy this link into your browser:</p>
            <p><small>{url}</small></p>
            <p>If you did not request this, you can safely ignore this email.</p>
            """;
    }

    private static string FormatErrors(IEnumerable<IdentityError> errors)
        => string.Join(" ", errors.Select(e => e.Description));
}
