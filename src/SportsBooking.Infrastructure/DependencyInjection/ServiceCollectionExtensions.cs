using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SportsBooking.Application.Interfaces;
using SportsBooking.Application.Options;
using SportsBooking.Domain.Entities;
using SportsBooking.Infrastructure.Auth;
using SportsBooking.Infrastructure.Email;
using SportsBooking.Infrastructure.Persistence;
using SportsBooking.Infrastructure.Payments;
using SportsBooking.Infrastructure.Repositories;

namespace SportsBooking.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

        services.AddDbContext<SportsBookingDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddDataProtection();

        services.AddIdentityCore<User>(options =>
        {
            options.SignIn.RequireConfirmedEmail = true;
            options.User.RequireUniqueEmail = true;

            options.Password.RequiredLength = 6;
            options.Password.RequireDigit = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireNonAlphanumeric = false;

            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
        })
            .AddRoles<IdentityRole<int>>()
            .AddEntityFrameworkStores<SportsBookingDbContext>()
            .AddTokenProvider<EmailConfirmationTokenProvider<User>>("EmailDataProtectorTokenProvider")
            .AddTokenProvider<DataProtectorTokenProvider<User>>("Default");

        services.Configure<IdentityOptions>(options =>
        {
            options.Tokens.EmailConfirmationTokenProvider = "EmailDataProtectorTokenProvider";
        });

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IFieldRepository, FieldRepository>();
        services.AddScoped<ISportRepository, SportRepository>();
        services.AddScoped<ILocationRepository, LocationRepository>();
        services.AddScoped<IFacilityRepository, FacilityRepository>();
        services.AddScoped<IFieldAvailabilityRepository, FieldAvailabilityRepository>();
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<IReviewRepository, ReviewRepository>();
        services.AddScoped<IFavoriteRepository, FavoriteRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();

        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IEmailSender, EmailSender>();
        services.AddScoped<IPaymentProvider, MockPaymentProvider>();

        return services;
    }
}
