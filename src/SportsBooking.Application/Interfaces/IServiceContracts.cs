using SportsBooking.Application.DTOs;
using SportsBooking.Domain.Enums;

namespace SportsBooking.Application.Interfaces;

public interface ITokenService
{
    string CreateToken(int userId, string email, UserRole role);
    string CreateRefreshToken();
    string HashRefreshToken(string token);
}

public interface IAuthService
{
    Task<RegisterResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default);
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default);
    Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken ct = default);
    Task RevokeTokenAsync(string refreshToken, CancellationToken ct = default);
    Task<MessageResponse> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken ct = default);
    Task<MessageResponse> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken ct = default);
    Task<MessageResponse> ChangePasswordAsync(int userId, ChangePasswordRequest request, CancellationToken ct = default);
    Task<MessageResponse> ConfirmEmailAsync(ConfirmEmailRequest request, CancellationToken ct = default);
    Task<MessageResponse> ResendConfirmationAsync(ResendConfirmationRequest request, CancellationToken ct = default);
}

public interface IUserService
{
    Task<UserProfileDto> GetProfileAsync(int userId, CancellationToken ct = default);
    Task<UserProfileDto> UpdateProfileAsync(int userId, UpdateProfileRequest request, CancellationToken ct = default);
    Task<UserProfileDto> UpdateLocationAsync(int userId, UpdateLocationRequest request, CancellationToken ct = default);
    Task<UserStatsDto> GetStatsAsync(int userId, CancellationToken ct = default);
    Task DeactivateAsync(int userId, CancellationToken ct = default);
}

public interface ISportService
{
    Task<IReadOnlyCollection<SportDto>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyCollection<SportDto>> GetAllIncludingInactiveAsync(CancellationToken ct = default);
    Task<SportDto> GetByIdAsync(int id, CancellationToken ct = default);
    Task<SportDto> CreateAsync(CreateSportRequest request, CancellationToken ct = default);
    Task<SportDto> UpdateAsync(int id, UpdateSportRequest request, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}

public interface ILocationService
{
    Task<IReadOnlyCollection<LocationDto>> GetAllAsync(CancellationToken ct = default);
    Task<LocationDetailsDto> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyCollection<LocationDto>> GetNearbyAsync(NearbyLocationsQuery query, CancellationToken ct = default);
    Task<PagedResult<LocationDto>> GetPagedAsync(int page, int pageSize, CancellationToken ct = default);
    Task<LocationDto> CreateAsync(CreateLocationRequest request, CancellationToken ct = default);
    Task<LocationDto> UpdateAsync(int id, AdminUpdateLocationRequest request, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}

public interface IFacilityService
{
    Task<IReadOnlyCollection<FacilityDto>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyCollection<FacilityDto>> GetAllActiveAsync(CancellationToken ct = default);
    Task<FacilityDto> GetByIdAsync(int id, CancellationToken ct = default);
    Task<FacilityDto> CreateAsync(CreateFacilityRequest request, CancellationToken ct = default);
    Task<FacilityDto> UpdateAsync(int id, UpdateFacilityRequest request, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}

public interface INotificationService
{
    Task<NotificationSummaryDto> GetAsync(int userId, int page, int pageSize, CancellationToken ct = default);
    Task<NotificationDto> MarkReadAsync(int userId, int notificationId, CancellationToken ct = default);
    Task MarkAllReadAsync(int userId, CancellationToken ct = default);
    Task<int> GetUnreadCountAsync(int userId, CancellationToken ct = default);
    Task DeleteAsync(int userId, int notificationId, CancellationToken ct = default);
    Task CreateAsync(int userId, string title, string message, NotificationType type, CancellationToken ct = default);
    Task<int> BroadcastAsync(string title, string message, CancellationToken ct = default);
}

public interface IOwnerFieldService
{
    Task<IReadOnlyCollection<FieldManagementDto>> GetMyFieldsAsync(int ownerId, CancellationToken ct = default);
    Task<FieldManagementDto> GetByIdAsync(int ownerId, int fieldId, CancellationToken ct = default);
    Task<FieldManagementDto> CreateAsync(int ownerId, CreateFieldRequest request, CancellationToken ct = default);
    Task<FieldManagementDto> UpdateAsync(int ownerId, int fieldId, UpdateFieldRequest request, CancellationToken ct = default);
    Task DeleteAsync(int ownerId, int fieldId, CancellationToken ct = default);
    Task<FieldAvailabilityDto> SetAvailabilityAsync(int ownerId, int fieldId, SetAvailabilityRequest request, CancellationToken ct = default);
    Task<FieldAvailabilityDto> UpdateAvailabilityAsync(int ownerId, int fieldId, int availabilityId, UpdateAvailabilityRequest request, CancellationToken ct = default);
    Task DeleteAvailabilityAsync(int ownerId, int fieldId, int availabilityId, CancellationToken ct = default);
}

public interface IOwnerBookingService
{
    Task<IReadOnlyCollection<OwnerBookingDto>> GetFieldBookingsAsync(int ownerId, int fieldId, CancellationToken ct = default);
    Task<OwnerBookingDto> GetByIdAsync(int ownerId, int bookingId, CancellationToken ct = default);
    Task<OwnerBookingDto> UpdateStatusAsync(int ownerId, int bookingId, UpdateOwnerBookingStatusRequest request, CancellationToken ct = default);
}

public interface IOwnerDashboardService
{
    Task<OwnerDashboardStatsDto> GetStatsAsync(int ownerId, CancellationToken ct = default);
    Task<OwnerRevenueDto> GetRevenueAsync(int ownerId, int days, CancellationToken ct = default);
    Task<IReadOnlyCollection<OwnerFieldPerformanceDto>> GetFieldPerformanceAsync(int ownerId, CancellationToken ct = default);
    Task<IReadOnlyCollection<OwnerBookingDto>> GetUpcomingBookingsAsync(int ownerId, CancellationToken ct = default);
}

public interface IAdminUserService
{
    Task<PagedResult<AdminUserDto>> GetUsersAsync(int page, int pageSize, string? search, CancellationToken ct = default);
    Task<AdminUserDto> GetByIdAsync(int id, CancellationToken ct = default);
    Task<AdminUserDto> SetStatusAsync(int id, UpdateUserStatusRequest request, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}

public interface IAdminFieldService
{
    Task<PagedResult<AdminFieldDto>> GetFieldsAsync(int page, int pageSize, bool? pendingOnly, CancellationToken ct = default);
    Task<AdminFieldDto> GetByIdAsync(int id, CancellationToken ct = default);
    Task<AdminFieldDto> SetApprovalAsync(int id, SetFieldApprovalRequest request, CancellationToken ct = default);
    Task<AdminFieldDto> SetStatusAsync(int id, UpdateUserStatusRequest request, CancellationToken ct = default);
}

public interface IAdminBookingService
{
    Task<PagedResult<AdminBookingDto>> GetBookingsAsync(int page, int pageSize, BookingStatus? status, CancellationToken ct = default);
    Task<AdminBookingDto> GetByIdAsync(int id, CancellationToken ct = default);
    Task<AdminBookingDto> CancelAsync(int id, string? reason, CancellationToken ct = default);
}

public interface IAdminDashboardService
{
    Task<AdminDashboardStatsDto> GetStatsAsync(CancellationToken ct = default);
    Task<AdminTrendsDto> GetTrendsAsync(int days, CancellationToken ct = default);
}

public interface IAdminPaymentService
{
    Task<PagedResult<AdminPaymentDto>> GetPaymentsAsync(int page, int pageSize, PaymentStatus? status, CancellationToken ct = default);
    Task<AdminPaymentDto> GetByIdAsync(int id, CancellationToken ct = default);
    Task<AdminPaymentDto> RefundAsync(int id, RefundPaymentRequest request, CancellationToken ct = default);
}

public interface IAdminReviewService
{
    Task<PagedResult<AdminReviewDto>> GetReviewsAsync(int page, int pageSize, CancellationToken ct = default);
    Task<AdminReviewDto> GetByIdAsync(int id, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}

public interface IAdminAuditLogService
{
    Task<PagedResult<AuditLogDto>> GetLogsAsync(int page, int pageSize, CancellationToken ct = default);
}

public interface IAdminReportService
{
    Task<AdminReportDto> GetReportAsync(CancellationToken ct = default);
}

public interface IAuditLogService
{
    Task LogAsync(int? userId, string action, string entityName, string entityId, string? details, CancellationToken ct = default);
}

public interface IFieldService
{
    Task<IReadOnlyCollection<FieldListItemDto>> GetAllAsync(CancellationToken ct = default);
    Task<FieldDetailsDto> GetByIdAsync(int id, CancellationToken ct = default);
    Task<PagedResult<FieldListItemDto>> SearchAsync(SearchFieldsQuery query, CancellationToken ct = default);
    Task<IReadOnlyCollection<FieldListItemDto>> GetNearbyAsync(NearbyFieldsQuery query, CancellationToken ct = default);
    Task<IReadOnlyCollection<FieldListItemDto>> GetTopRatedAsync(double latitude, double longitude, double? radiusKm, CancellationToken ct = default);
    Task<IReadOnlyCollection<FieldListItemDto>> GetFeaturedAsync(int count, CancellationToken ct = default);
    Task<IReadOnlyCollection<FieldCityDto>> GetCitiesAsync(CancellationToken ct = default);
    Task<IReadOnlyCollection<FieldListItemDto>> GetSimilarAsync(int fieldId, int count, CancellationToken ct = default);
    Task<IReadOnlyCollection<FieldAvailabilityDto>> GetScheduleAsync(int fieldId, DateOnly startDate, int days, CancellationToken ct = default);
    Task<FieldAvailabilityDto> GetAvailabilityAsync(int fieldId, DateOnly date, CancellationToken ct = default);
    Task<IReadOnlyCollection<ReviewDto>> GetReviewsAsync(int fieldId, CancellationToken ct = default);
}

public interface IBookingService
{
    Task<BookingPreviewDto> PreviewAsync(int fieldId, DateOnly date, TimeOnly startTime, int durationHours, CancellationToken ct = default);
    Task<BookingDto> CreateAsync(int userId, CreateBookingRequest request, CancellationToken ct = default);
    Task<IReadOnlyCollection<BookingDto>> GetUserBookingsAsync(int userId, CancellationToken ct = default);
    Task<IReadOnlyCollection<BookingDto>> GetUpcomingAsync(int userId, CancellationToken ct = default);
    Task<IReadOnlyCollection<BookingDto>> GetPastAsync(int userId, CancellationToken ct = default);
    Task<BookingStatsDto> GetStatsAsync(int userId, CancellationToken ct = default);
    Task<BookingDto> GetByIdAsync(int userId, int bookingId, CancellationToken ct = default);
    Task<BookingDto> CancelAsync(int userId, int bookingId, string? reason, CancellationToken ct = default);
}

public interface IReviewService
{
    Task<ReviewDto> CreateAsync(int userId, CreateReviewRequest request, CancellationToken ct = default);
    Task<ReviewDto> UpdateAsync(int userId, int reviewId, UpdateReviewRequest request, CancellationToken ct = default);
    Task DeleteAsync(int userId, int reviewId, CancellationToken ct = default);
    Task<IReadOnlyCollection<ReviewDto>> GetMyReviewsAsync(int userId, CancellationToken ct = default);
}

public interface IFavoriteService
{
    Task AddAsync(int userId, int fieldId, CancellationToken ct = default);
    Task RemoveAsync(int userId, int fieldId, CancellationToken ct = default);
    Task<IReadOnlyCollection<FavoriteDto>> GetAsync(int userId, CancellationToken ct = default);
    Task<bool> ExistsAsync(int userId, int fieldId, CancellationToken ct = default);
    Task<int> CountAsync(int userId, CancellationToken ct = default);
}

public interface IPaymentService
{
    Task<PaymentResponse> CreateAsync(int userId, CreatePaymentRequest request, CancellationToken ct = default);
    Task<PaymentResponse> GetByIdAsync(int userId, int paymentId, CancellationToken ct = default);
    Task<PaymentStatusResponse> GetStatusAsync(int userId, int paymentId, CancellationToken ct = default);
    Task<PagedResult<PaymentResponse>> GetUserPaymentsAsync(int userId, int page, int pageSize, CancellationToken ct = default);
    Task<PaymentResponse> ProcessWebhookAsync(PaymentWebhookRequest request, CancellationToken ct = default);
    Task<PaymentResponse> RefundAsync(int userId, int paymentId, RefundPaymentRequest request, CancellationToken ct = default);
}