using SportsBooking.Domain.Entities;
using SportsBooking.Domain.Enums;

namespace SportsBooking.Application.Interfaces;

public interface IUserRepository : IUnitOfWork
{
    Task<User?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken ct = default);
    Task<IReadOnlyCollection<User>> GetPagedAsync(int page, int pageSize, string? search, CancellationToken ct = default);
    Task<int> CountAsync(CancellationToken ct = default);
    Task AddAsync(User user, CancellationToken ct = default);
    void Remove(User user);
}

public interface IFieldRepository : IUnitOfWork
{
    Task<Field?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Field?> GetFieldDetailsAsync(int id, CancellationToken ct = default);
    Task<Field?> GetByIdWithFacilitiesAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyCollection<Field>> GetAllFieldsAsync(CancellationToken ct = default);
    Task<IReadOnlyCollection<Field>> GetByIdsAsync(IReadOnlyCollection<int> ids, CancellationToken ct = default);
    Task<IReadOnlyCollection<Field>> GetOwnerFieldsAsync(int ownerId, CancellationToken ct = default);
    Task AddAsync(Field field, CancellationToken ct = default);
    void Update(Field field);
    void Remove(Field field);
}

public interface ISportRepository : IUnitOfWork
{
    Task<IReadOnlyCollection<Sport>> GetAllAsync(CancellationToken ct = default);
    Task<Sport?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<bool> SlugExistsAsync(string slug, int? excludeId, CancellationToken ct = default);
    Task AddAsync(Sport sport, CancellationToken ct = default);
    void Update(Sport sport);
    void Remove(Sport sport);
}

public interface IBookingRepository : IUnitOfWork
{
    Task<Booking?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyCollection<Booking>> GetUserBookingsAsync(int userId, CancellationToken ct = default);
    Task<bool> HasConflictingBookingAsync(int fieldId, DateOnly date, TimeOnly startTime, TimeOnly endTime, CancellationToken ct = default);
    Task<IReadOnlyCollection<Booking>> GetFieldBookingsByDateAsync(int fieldId, DateOnly date, CancellationToken ct = default);
    Task<(IReadOnlyCollection<Booking> Items, int Total)> GetPagedAsync(int page, int pageSize, BookingStatus? status, CancellationToken ct = default);
    Task<IReadOnlyCollection<Booking>> GetFieldBookingsAsync(int fieldId, CancellationToken ct = default);
    Task AddAsync(Booking booking, CancellationToken ct = default);
    Task<ITransaction> BeginTransactionAsync(CancellationToken ct = default);
}

public interface IReviewRepository : IUnitOfWork
{
    Task<Review?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Review?> GetByBookingIdAsync(int bookingId, CancellationToken ct = default);
    Task<IReadOnlyCollection<Review>> GetFieldReviewsAsync(int fieldId, CancellationToken ct = default);
    Task<IReadOnlyCollection<Review>> GetByUserAsync(int userId, CancellationToken ct = default);
    Task<(IReadOnlyCollection<Review> Items, int Total)> GetPagedAsync(int page, int pageSize, CancellationToken ct = default);
    Task AddAsync(Review review, CancellationToken ct = default);
    void Remove(Review review);
}

public interface IFavoriteRepository : IUnitOfWork
{
    Task<Favorite?> GetAsync(int userId, int fieldId, CancellationToken ct = default);
    Task<bool> ExistsAsync(int userId, int fieldId, CancellationToken ct = default);
    Task<int> CountAsync(int userId, CancellationToken ct = default);
    Task<IReadOnlyCollection<Favorite>> GetUserFavoritesAsync(int userId, CancellationToken ct = default);
    Task AddAsync(Favorite favorite, CancellationToken ct = default);
    void Remove(Favorite favorite);
}

public interface ILocationRepository : IUnitOfWork
{
    Task<IReadOnlyCollection<Location>> GetAllAsync(CancellationToken ct = default);
    Task<Location?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<(IReadOnlyCollection<Location> Items, int Total)> GetPagedAsync(int page, int pageSize, CancellationToken ct = default);
    Task<bool> IsInUseAsync(int id, CancellationToken ct = default);
    Task AddAsync(Location location, CancellationToken ct = default);
    void Update(Location location);
    void Remove(Location location);
}

public interface IFacilityRepository : IUnitOfWork
{
    Task<IReadOnlyCollection<Facility>> GetAllAsync(CancellationToken ct = default);
    Task<Facility?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyCollection<Facility>> GetByIdsAsync(IReadOnlyCollection<int> ids, CancellationToken ct = default);
    Task<bool> ExistsAsync(int id, CancellationToken ct = default);
    Task<bool> IsInUseAsync(int id, CancellationToken ct = default);
    Task AddAsync(Facility facility, CancellationToken ct = default);
    void Update(Facility facility);
    void Remove(Facility facility);
}

public interface INotificationRepository : IUnitOfWork
{
    Task<IReadOnlyCollection<Notification>> GetByUserIdAsync(int userId, int skip, int take, CancellationToken ct = default);
    Task<int> CountUnreadAsync(int userId, CancellationToken ct = default);
    Task<int> CountByUserIdAsync(int userId, CancellationToken ct = default);
    Task<Notification?> GetByIdAsync(int id, CancellationToken ct = default);
    Task AddAsync(Notification notification, CancellationToken ct = default);
    void Remove(Notification notification);
}

public interface IAuditLogRepository : IUnitOfWork
{
    Task AddAsync(AuditLog auditLog, CancellationToken ct = default);
    Task<(IReadOnlyCollection<AuditLog> Items, int Total)> GetPagedAsync(int page, int pageSize, CancellationToken ct = default);
}

public interface IFieldAvailabilityRepository : IUnitOfWork
{
    Task<FieldAvailability?> GetByFieldAndDateAsync(int fieldId, DateOnly date, CancellationToken ct = default);
    Task<IReadOnlyCollection<FieldAvailability>> GetByFieldAsync(int fieldId, CancellationToken ct = default);
    Task<FieldAvailability?> GetByIdAsync(int id, CancellationToken ct = default);
    Task AddAsync(FieldAvailability availability, CancellationToken ct = default);
    void Remove(FieldAvailability availability);
}

public interface IRefreshTokenRepository : IUnitOfWork
{
    Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken ct = default);
    Task<IReadOnlyCollection<RefreshToken>> GetActiveByUserIdAsync(int userId, CancellationToken ct = default);
    Task AddAsync(RefreshToken token, CancellationToken ct = default);
    void Remove(RefreshToken token);
}

public interface IPaymentRepository : IUnitOfWork
{
    Task<Payment?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Payment?> GetByTransactionIdAsync(string transactionId, CancellationToken ct = default);
    Task<IReadOnlyCollection<Payment>> GetByBookingIdAsync(int bookingId, CancellationToken ct = default);
    Task<Payment?> GetLatestByBookingIdAsync(int bookingId, CancellationToken ct = default);
    Task<(IReadOnlyCollection<Payment> Items, int Total)> GetByUserIdPagedAsync(int userId, int page, int pageSize, CancellationToken ct = default);
    Task<(IReadOnlyCollection<Payment> Items, int Total)> GetPagedAsync(int page, int pageSize, PaymentStatus? status, CancellationToken ct = default);
    Task AddAsync(Payment payment, CancellationToken ct = default);
    Task<ITransaction> BeginTransactionAsync(CancellationToken ct = default);
}