namespace SportsBooking.Domain.Enums;

public enum UserRole
{
    Customer = 1,
    Owner = 2,
    Admin = 3
}

public enum SportType
{
    Football = 1,
    Padel = 2,
    Basketball = 3,
    Tennis = 4,
    Handball = 5
}

public enum FieldType
{
    Indoor = 1,
    Outdoor = 2
}

public enum BookingStatus
{
    Pending = 1,
    Confirmed = 2,
    Completed = 3,
    Cancelled = 4,
    PendingPayment = 5,
    Expired = 6
}

public enum PaymentStatus
{
    Pending = 1,
    Paid = 2,
    Failed = 3,
    Refunded = 4,
    Cancelled = 5
}

public enum PaymentMethod
{
    Card = 1,
    Wallet = 2,
    Cash = 3,
    BankTransfer = 4
}

public enum DayPeriod
{
    Day = 1,
    Night = 2
}

public enum SortBy
{
    Distance = 1,
    Rating = 2,
    PriceAsc = 3,
    PriceDesc = 4,
    ReviewCount = 5
}

public enum NotificationType
{
    BookingCreated = 1,
    BookingConfirmed = 2,
    BookingCancelled = 3,
    BookingCompleted = 4,
    PaymentSucceeded = 5,
    PaymentFailed = 6,
    PaymentRefunded = 7,
    FieldApproved = 8,
    FieldRejected = 9,
    System = 10
}

public static class AppRoles
{
    public const string Customer = nameof(UserRole.Customer);
    public const string Owner = nameof(UserRole.Owner);
    public const string Admin = nameof(UserRole.Admin);
}

public static class BookingStatusExtensions
{
    public static readonly BookingStatus[] OccupyingStatuses =
    {
        BookingStatus.PendingPayment, BookingStatus.Confirmed, BookingStatus.Pending
    };

    public static bool OccupiesSlot(this BookingStatus status)
        => status is BookingStatus.PendingPayment or BookingStatus.Confirmed or BookingStatus.Pending;
}