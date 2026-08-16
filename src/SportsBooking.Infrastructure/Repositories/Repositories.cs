using Microsoft.EntityFrameworkCore;
using SportsBooking.Application.Interfaces;
using SportsBooking.Domain.Entities;
using SportsBooking.Domain.Enums;
using SportsBooking.Infrastructure.Persistence;

namespace SportsBooking.Infrastructure.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly SportsBookingDbContext _db;

    public UserRepository(SportsBookingDbContext db) => _db = db;

    public Task<User?> GetByIdAsync(int id, CancellationToken ct = default)
        => _db.Users.FirstOrDefaultAsync(u => u.Id == id, ct);

    public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
        => _db.Users.FirstOrDefaultAsync(u => u.Email == email, ct);

    public Task<bool> EmailExistsAsync(string email, CancellationToken ct = default)
        => _db.Users.AnyAsync(u => u.Email == email, ct);

    public async Task<IReadOnlyCollection<User>> GetPagedAsync(int page, int pageSize, string? search, CancellationToken ct = default)
    {
        var query = _db.Users.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();
            query = query.Where(u =>
                (u.FirstName + " " + u.LastName).Contains(search) ||
                (u.Email != null && u.Email.Contains(search)));
        }

        var users = await query
            .OrderBy(u => u.CreatedAtUtc)
            .Skip((Math.Max(page, 1) - 1) * Math.Clamp(pageSize, 1, 100))
            .Take(Math.Clamp(pageSize, 1, 100))
            .ToListAsync(ct);

        return users;
    }

    public Task<int> CountAsync(CancellationToken ct = default)
        => _db.Users.CountAsync(ct);

    public async Task AddAsync(User user, CancellationToken ct = default)
        => await _db.Users.AddAsync(user, ct);

    public void Remove(User user) => _db.Users.Remove(user);

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}

public sealed class FieldRepository : IFieldRepository
{
    private readonly SportsBookingDbContext _db;

    public FieldRepository(SportsBookingDbContext db) => _db = db;

    public Task<Field?> GetByIdAsync(int id, CancellationToken ct = default)
        => _db.Fields
            .Include(f => f.Sport)
            .Include(f => f.Location)
            .FirstOrDefaultAsync(f => f.Id == id, ct);

    public Task<Field?> GetFieldDetailsAsync(int id, CancellationToken ct = default)
        => _db.Fields
            .Include(f => f.Sport)
            .Include(f => f.Location)
            .Include(f => f.Owner)
            .Include(f => f.Images)
            .Include(f => f.Amenities)
            .FirstOrDefaultAsync(f => f.Id == id, ct);

    public Task<Field?> GetByIdWithFacilitiesAsync(int id, CancellationToken ct = default)
        => _db.Fields
            .Include(f => f.Sport)
            .Include(f => f.Location)
            .Include(f => f.Images)
            .Include(f => f.Amenities)
            .Include(f => f.Facilities)
                .ThenInclude(x => x.Facility)
            .FirstOrDefaultAsync(f => f.Id == id, ct);

    public Task<IReadOnlyCollection<Field>> GetAllFieldsAsync(CancellationToken ct = default)
        => _db.Fields
            .Include(f => f.Sport)
            .Include(f => f.Location)
            .Include(f => f.Owner)
            .Include(f => f.Images)
            .Include(f => f.Amenities)
            .Include(f => f.Facilities)
                .ThenInclude(x => x.Facility)
            .Include(f => f.Bookings)
            .AsNoTracking()
            .ToListAsync(ct)
            .ContinueWith(t => (IReadOnlyCollection<Field>)t.Result, ct);

    public async Task<IReadOnlyCollection<Field>> GetOwnerFieldsAsync(int ownerId, CancellationToken ct = default)
    {
        var fields = await _db.Fields
            .Include(f => f.Sport)
            .Include(f => f.Location)
            .Include(f => f.Images)
            .Include(f => f.Amenities)
            .Include(f => f.Facilities)
                .ThenInclude(x => x.Facility)
            .Where(f => f.OwnerId == ownerId)
            .OrderByDescending(f => f.CreatedAtUtc)
            .ToListAsync(ct);
        return fields;
    }

    public async Task<IReadOnlyCollection<Field>> GetByIdsAsync(IReadOnlyCollection<int> ids, CancellationToken ct = default)
    {
        var fields = await _db.Fields
            .Include(f => f.Sport)
            .Include(f => f.Images)
            .Where(f => ids.Contains(f.Id))
            .ToListAsync(ct);
        return fields;
    }

    public async Task AddAsync(Field field, CancellationToken ct = default)
        => await _db.Fields.AddAsync(field, ct);

    public void Update(Field field) => _db.Fields.Update(field);

    public void Remove(Field field) => _db.Fields.Remove(field);

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}

public sealed class SportRepository : ISportRepository
{
    private readonly SportsBookingDbContext _db;

    public SportRepository(SportsBookingDbContext db) => _db = db;

    public async Task<IReadOnlyCollection<Sport>> GetAllAsync(CancellationToken ct = default)
    {
        var sports = await _db.Sports.AsNoTracking().ToListAsync(ct);
        return sports;
    }

    public Task<Sport?> GetByIdAsync(int id, CancellationToken ct = default)
        => _db.Sports.FirstOrDefaultAsync(s => s.Id == id, ct);

    public Task<bool> SlugExistsAsync(string slug, int? excludeId, CancellationToken ct = default)
        => _db.Sports.AnyAsync(s => s.Slug == slug && (!excludeId.HasValue || s.Id != excludeId.Value), ct);

    public async Task AddAsync(Sport sport, CancellationToken ct = default)
        => await _db.Sports.AddAsync(sport, ct);

    public void Update(Sport sport) => _db.Sports.Update(sport);

    public void Remove(Sport sport) => _db.Sports.Remove(sport);

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}

public sealed class BookingRepository : IBookingRepository
{
    private readonly SportsBookingDbContext _db;

    public BookingRepository(SportsBookingDbContext db) => _db = db;

    public Task<Booking?> GetByIdAsync(int id, CancellationToken ct = default)
        => _db.Bookings
            .Include(b => b.Field)
            .Include(b => b.User)
            .FirstOrDefaultAsync(b => b.Id == id, ct);

    public async Task<IReadOnlyCollection<Booking>> GetUserBookingsAsync(int userId, CancellationToken ct = default)
    {
        var bookings = await _db.Bookings
            .Include(b => b.Field)
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.BookingDate)
            .ThenByDescending(b => b.StartTime)
            .ToListAsync(ct);
        return bookings;
    }

    public Task<bool> HasConflictingBookingAsync(int fieldId, DateOnly date, TimeOnly startTime, TimeOnly endTime, CancellationToken ct = default)
        => _db.Bookings.AnyAsync(b =>
            b.FieldId == fieldId &&
            b.BookingDate == date &&
            BookingStatusExtensions.OccupyingStatuses.Contains(b.Status) &&
            b.StartTime < endTime &&
            b.EndTime > startTime, ct);

    public async Task<IReadOnlyCollection<Booking>> GetFieldBookingsByDateAsync(int fieldId, DateOnly date, CancellationToken ct = default)
    {
        var bookings = await _db.Bookings
            .Where(b => b.FieldId == fieldId && b.BookingDate == date)
            .ToListAsync(ct);
        return bookings;
    }

    public async Task<(IReadOnlyCollection<Booking> Items, int Total)> GetPagedAsync(int page, int pageSize, BookingStatus? status, CancellationToken ct = default)
    {
        var query = _db.Bookings
            .Include(b => b.Field)
            .Include(b => b.User)
            .AsNoTracking();

        if (status.HasValue)
        {
            query = query.Where(b => b.Status == status.Value);
        }

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(b => b.CreatedAtUtc)
            .Skip((Math.Max(page, 1) - 1) * Math.Clamp(pageSize, 1, 100))
            .Take(Math.Clamp(pageSize, 1, 100))
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task<IReadOnlyCollection<Booking>> GetFieldBookingsAsync(int fieldId, CancellationToken ct = default)
    {
        var bookings = await _db.Bookings
            .Include(b => b.Field)
            .Include(b => b.User)
            .Where(b => b.FieldId == fieldId)
            .ToListAsync(ct);
        return bookings;
    }

    public async Task AddAsync(Booking booking, CancellationToken ct = default)
        => await _db.Bookings.AddAsync(booking, ct);

    public async Task<ITransaction> BeginTransactionAsync(CancellationToken ct = default)
    {
        var transaction = await _db.Database.BeginTransactionAsync(ct);
        return new EfTransaction(transaction);
    }

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}

public sealed class ReviewRepository : IReviewRepository
{
    private readonly SportsBookingDbContext _db;

    public ReviewRepository(SportsBookingDbContext db) => _db = db;

    public Task<Review?> GetByIdAsync(int id, CancellationToken ct = default)
        => _db.Reviews
            .Include(r => r.User)
            .Include(r => r.Field)
            .FirstOrDefaultAsync(r => r.Id == id, ct);

    public Task<Review?> GetByBookingIdAsync(int bookingId, CancellationToken ct = default)
        => _db.Reviews.FirstOrDefaultAsync(r => r.BookingId == bookingId, ct);

    public async Task<IReadOnlyCollection<Review>> GetFieldReviewsAsync(int fieldId, CancellationToken ct = default)
    {
        var reviews = await _db.Reviews
            .Include(r => r.User)
            .Where(r => r.FieldId == fieldId)
            .OrderByDescending(r => r.CreatedAtUtc)
            .ToListAsync(ct);
        return reviews;
    }

    public async Task<IReadOnlyCollection<Review>> GetByUserAsync(int userId, CancellationToken ct = default)
    {
        var reviews = await _db.Reviews
            .Include(r => r.User)
            .Include(r => r.Field)
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAtUtc)
            .ToListAsync(ct);
        return reviews;
    }

    public async Task<(IReadOnlyCollection<Review> Items, int Total)> GetPagedAsync(int page, int pageSize, CancellationToken ct = default)
    {
        var query = _db.Reviews
            .Include(r => r.User)
            .Include(r => r.Field)
            .AsNoTracking();

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(r => r.CreatedAtUtc)
            .Skip((Math.Max(page, 1) - 1) * Math.Clamp(pageSize, 1, 100))
            .Take(Math.Clamp(pageSize, 1, 100))
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task AddAsync(Review review, CancellationToken ct = default)
        => await _db.Reviews.AddAsync(review, ct);

    public void Remove(Review review) => _db.Reviews.Remove(review);

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}

public sealed class FavoriteRepository : IFavoriteRepository
{
    private readonly SportsBookingDbContext _db;

    public FavoriteRepository(SportsBookingDbContext db) => _db = db;

    public Task<Favorite?> GetAsync(int userId, int fieldId, CancellationToken ct = default)
        => _db.Favorites.FirstOrDefaultAsync(f => f.UserId == userId && f.FieldId == fieldId, ct);

    public Task<bool> ExistsAsync(int userId, int fieldId, CancellationToken ct = default)
        => _db.Favorites.AnyAsync(f => f.UserId == userId && f.FieldId == fieldId, ct);

    public Task<int> CountAsync(int userId, CancellationToken ct = default)
        => _db.Favorites.CountAsync(f => f.UserId == userId, ct);

    public async Task<IReadOnlyCollection<Favorite>> GetUserFavoritesAsync(int userId, CancellationToken ct = default)
    {
        var favorites = await _db.Favorites
            .Include(f => f.Field)
                .ThenInclude(f => f.Images)
            .Where(f => f.UserId == userId)
            .OrderByDescending(f => f.CreatedAtUtc)
            .ToListAsync(ct);
        return favorites;
    }

    public async Task AddAsync(Favorite favorite, CancellationToken ct = default)
        => await _db.Favorites.AddAsync(favorite, ct);

    public void Remove(Favorite favorite) => _db.Favorites.Remove(favorite);

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}

public sealed class LocationRepository : ILocationRepository
{
    private readonly SportsBookingDbContext _db;

    public LocationRepository(SportsBookingDbContext db) => _db = db;

    public async Task<IReadOnlyCollection<Location>> GetAllAsync(CancellationToken ct = default)
    {
        var locations = await _db.Locations.AsNoTracking().ToListAsync(ct);
        return locations;
    }

    public Task<Location?> GetByIdAsync(int id, CancellationToken ct = default)
        => _db.Locations.FirstOrDefaultAsync(l => l.Id == id, ct);

    public async Task<(IReadOnlyCollection<Location> Items, int Total)> GetPagedAsync(int page, int pageSize, CancellationToken ct = default)
    {
        var query = _db.Locations.AsNoTracking();

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(l => l.Governorate)
            .ThenBy(l => l.Name)
            .Skip((Math.Max(page, 1) - 1) * Math.Clamp(pageSize, 1, 100))
            .Take(Math.Clamp(pageSize, 1, 100))
            .ToListAsync(ct);

        return (items, total);
    }

    public Task<bool> IsInUseAsync(int id, CancellationToken ct = default)
        => _db.Fields.AnyAsync(f => f.LocationId == id, ct);

    public async Task AddAsync(Location location, CancellationToken ct = default)
        => await _db.Locations.AddAsync(location, ct);

    public void Update(Location location) => _db.Locations.Update(location);

    public void Remove(Location location) => _db.Locations.Remove(location);

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}

public sealed class FieldAvailabilityRepository : IFieldAvailabilityRepository
{
    private readonly SportsBookingDbContext _db;

    public FieldAvailabilityRepository(SportsBookingDbContext db) => _db = db;

    public Task<FieldAvailability?> GetByFieldAndDateAsync(int fieldId, DateOnly date, CancellationToken ct = default)
        => _db.FieldAvailabilities.FirstOrDefaultAsync(a => a.FieldId == fieldId && a.Date == date, ct);

    public async Task<IReadOnlyCollection<FieldAvailability>> GetByFieldAsync(int fieldId, CancellationToken ct = default)
    {
        var items = await _db.FieldAvailabilities
            .Where(a => a.FieldId == fieldId)
            .OrderBy(a => a.Date)
            .ToListAsync(ct);
        return items;
    }

    public Task<FieldAvailability?> GetByIdAsync(int id, CancellationToken ct = default)
        => _db.FieldAvailabilities.FirstOrDefaultAsync(a => a.Id == id, ct);

    public async Task AddAsync(FieldAvailability availability, CancellationToken ct = default)
        => await _db.FieldAvailabilities.AddAsync(availability, ct);

    public void Remove(FieldAvailability availability) => _db.FieldAvailabilities.Remove(availability);

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}

public sealed class FacilityRepository : IFacilityRepository
{
    private readonly SportsBookingDbContext _db;

    public FacilityRepository(SportsBookingDbContext db) => _db = db;

    public async Task<IReadOnlyCollection<Facility>> GetAllAsync(CancellationToken ct = default)
    {
        var facilities = await _db.Facilities.AsNoTracking().OrderBy(f => f.Name).ToListAsync(ct);
        return facilities;
    }

    public Task<Facility?> GetByIdAsync(int id, CancellationToken ct = default)
        => _db.Facilities.FirstOrDefaultAsync(f => f.Id == id, ct);

    public async Task<IReadOnlyCollection<Facility>> GetByIdsAsync(IReadOnlyCollection<int> ids, CancellationToken ct = default)
    {
        var facilities = await _db.Facilities
            .Where(f => ids.Contains(f.Id))
            .ToListAsync(ct);
        return facilities;
    }

    public Task<bool> ExistsAsync(int id, CancellationToken ct = default)
        => _db.Facilities.AnyAsync(f => f.Id == id, ct);

    public Task<bool> IsInUseAsync(int id, CancellationToken ct = default)
        => _db.FieldFacilities.AnyAsync(x => x.FacilityId == id, ct);

    public async Task AddAsync(Facility facility, CancellationToken ct = default)
        => await _db.Facilities.AddAsync(facility, ct);

    public void Update(Facility facility) => _db.Facilities.Update(facility);

    public void Remove(Facility facility) => _db.Facilities.Remove(facility);

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}

public sealed class NotificationRepository : INotificationRepository
{
    private readonly SportsBookingDbContext _db;

    public NotificationRepository(SportsBookingDbContext db) => _db = db;

    public async Task<IReadOnlyCollection<Notification>> GetByUserIdAsync(int userId, int skip, int take, CancellationToken ct = default)
    {
        var items = await _db.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAtUtc)
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);
        return items;
    }

    public Task<int> CountUnreadAsync(int userId, CancellationToken ct = default)
        => _db.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead, ct);

    public Task<int> CountByUserIdAsync(int userId, CancellationToken ct = default)
        => _db.Notifications.CountAsync(n => n.UserId == userId, ct);

    public Task<Notification?> GetByIdAsync(int id, CancellationToken ct = default)
        => _db.Notifications.FirstOrDefaultAsync(n => n.Id == id, ct);

    public async Task AddAsync(Notification notification, CancellationToken ct = default)
        => await _db.Notifications.AddAsync(notification, ct);

    public void Remove(Notification notification) => _db.Notifications.Remove(notification);

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}

public sealed class AuditLogRepository : IAuditLogRepository
{
    private readonly SportsBookingDbContext _db;

    public AuditLogRepository(SportsBookingDbContext db) => _db = db;

    public async Task AddAsync(AuditLog auditLog, CancellationToken ct = default)
        => await _db.AuditLogs.AddAsync(auditLog, ct);

    public async Task<(IReadOnlyCollection<AuditLog> Items, int Total)> GetPagedAsync(int page, int pageSize, CancellationToken ct = default)
    {
        var query = _db.AuditLogs.AsNoTracking();

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(l => l.CreatedAtUtc)
            .Skip((Math.Max(page, 1) - 1) * Math.Clamp(pageSize, 1, 100))
            .Take(Math.Clamp(pageSize, 1, 100))
            .ToListAsync(ct);

        return (items, total);
    }

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}

public sealed class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly SportsBookingDbContext _db;

    public RefreshTokenRepository(SportsBookingDbContext db) => _db = db;

    public Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken ct = default)
        => _db.RefreshTokens
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.TokenHash == tokenHash, ct);

    public async Task<IReadOnlyCollection<RefreshToken>> GetActiveByUserIdAsync(int userId, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var tokens = await _db.RefreshTokens
            .Where(r => r.UserId == userId && r.RevokedAtUtc == null && r.ExpiresAtUtc > now)
            .ToListAsync(ct);
        return tokens;
    }

    public async Task AddAsync(RefreshToken token, CancellationToken ct = default)
        => await _db.RefreshTokens.AddAsync(token, ct);

    public void Remove(RefreshToken token) => _db.RefreshTokens.Remove(token);

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}

public sealed class PaymentRepository : IPaymentRepository
{
    private readonly SportsBookingDbContext _db;

    public PaymentRepository(SportsBookingDbContext db) => _db = db;

    public Task<Payment?> GetByIdAsync(int id, CancellationToken ct = default)
        => _db.Payments
            .Include(p => p.Booking)
                .ThenInclude(b => b.Field)
            .Include(p => p.Booking)
                .ThenInclude(b => b.User)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

    public Task<Payment?> GetByTransactionIdAsync(string transactionId, CancellationToken ct = default)
        => _db.Payments
            .Include(p => p.Booking)
            .FirstOrDefaultAsync(p => p.TransactionId == transactionId, ct);

    public async Task<IReadOnlyCollection<Payment>> GetByBookingIdAsync(int bookingId, CancellationToken ct = default)
    {
        var payments = await _db.Payments
            .Include(p => p.Booking)
            .Where(p => p.BookingId == bookingId)
            .OrderByDescending(p => p.CreatedAtUtc)
            .ToListAsync(ct);
        return payments;
    }

    public Task<Payment?> GetLatestByBookingIdAsync(int bookingId, CancellationToken ct = default)
        => _db.Payments
            .Include(p => p.Booking)
            .Where(p => p.BookingId == bookingId)
            .OrderByDescending(p => p.CreatedAtUtc)
            .FirstOrDefaultAsync(ct);

    private IQueryable<Payment> PaymentsQuery()
        => _db.Payments
            .Include(p => p.Booking)
                .ThenInclude(b => b.Field)
            .Include(p => p.Booking)
                .ThenInclude(b => b.User)
            .AsNoTracking();

    public async Task<(IReadOnlyCollection<Payment> Items, int Total)> GetByUserIdPagedAsync(int userId, int page, int pageSize, CancellationToken ct = default)
    {
        var query = PaymentsQuery().Where(p => p.Booking.UserId == userId);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(p => p.CreatedAtUtc)
            .Skip((Math.Max(page, 1) - 1) * Math.Clamp(pageSize, 1, 100))
            .Take(Math.Clamp(pageSize, 1, 100))
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task<(IReadOnlyCollection<Payment> Items, int Total)> GetPagedAsync(int page, int pageSize, PaymentStatus? status, CancellationToken ct = default)
    {
        var query = PaymentsQuery();

        if (status.HasValue)
        {
            query = query.Where(p => p.Status == status.Value);
        }

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(p => p.CreatedAtUtc)
            .Skip((Math.Max(page, 1) - 1) * Math.Clamp(pageSize, 1, 100))
            .Take(Math.Clamp(pageSize, 1, 100))
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task AddAsync(Payment payment, CancellationToken ct = default)
        => await _db.Payments.AddAsync(payment, ct);

    public async Task<ITransaction> BeginTransactionAsync(CancellationToken ct = default)
    {
        var transaction = await _db.Database.BeginTransactionAsync(ct);
        return new EfTransaction(transaction);
    }

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}