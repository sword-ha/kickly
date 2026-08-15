using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SportsBooking.Application.Interfaces;
using SportsBooking.Application.Options;
using SportsBooking.Application.Services;
using SportsBooking.Application.Validators;

namespace SportsBooking.Application.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .Validate(o => !string.IsNullOrWhiteSpace(o.Key), "JWT key is required.")
            .ValidateOnStart();

        services.AddOptions<PricingOptions>()
            .Bind(configuration.GetSection(PricingOptions.SectionName))
            .ValidateOnStart();

        services.AddOptions<BookingOptions>()
            .Bind(configuration.GetSection(BookingOptions.SectionName))
            .ValidateOnStart();

        services.AddOptions<LocationOptions>()
            .Bind(configuration.GetSection(LocationOptions.SectionName))
            .ValidateOnStart();

        services.AddOptions<SmtpOptions>()
            .Bind(configuration.GetSection(SmtpOptions.SectionName))
            .ValidateOnStart();

        services.AddOptions<AppOptions>()
            .Bind(configuration.GetSection(AppOptions.SectionName))
            .ValidateOnStart();

        services.AddOptions<PaymentOptions>()
            .Bind(configuration.GetSection(PaymentOptions.SectionName))
            .ValidateOnStart();

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ISportService, SportService>();
        services.AddScoped<ILocationService, LocationService>();
        services.AddScoped<IFacilityService, FacilityService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IFieldService, FieldService>();
        services.AddScoped<IOwnerFieldService, OwnerFieldService>();
        services.AddScoped<IOwnerBookingService, OwnerBookingService>();
        services.AddScoped<IOwnerDashboardService, OwnerDashboardService>();
        services.AddScoped<IBookingService, BookingService>();
        services.AddScoped<IReviewService, ReviewService>();
        services.AddScoped<IFavoriteService, FavoriteService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IAuditLogService, AuditLogService>();
        services.AddScoped<IAdminUserService, AdminUserService>();
        services.AddScoped<IAdminFieldService, AdminFieldService>();
        services.AddScoped<IAdminBookingService, AdminBookingService>();
        services.AddScoped<IAdminDashboardService, AdminDashboardService>();
        services.AddScoped<IAdminReportService, AdminReportService>();

        services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();

        return services;
    }
}