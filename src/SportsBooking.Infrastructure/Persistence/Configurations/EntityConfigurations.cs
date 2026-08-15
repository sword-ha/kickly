using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportsBooking.Domain.Entities;

namespace SportsBooking.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.LastName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.PhoneNumber).HasMaxLength(20).IsRequired();
        builder.HasIndex(x => x.Email).IsUnique();
        builder.Property(x => x.Latitude).HasPrecision(10, 7);
        builder.Property(x => x.Longitude).HasPrecision(10, 7);
    }
}

public sealed class LocationConfiguration : IEntityTypeConfiguration<Location>
{
    public void Configure(EntityTypeBuilder<Location> builder)
    {
        builder.ToTable("Locations");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.City).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Governorate).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Address).HasMaxLength(500).IsRequired();
        builder.Property(x => x.Latitude).HasPrecision(10, 7).IsRequired();
        builder.Property(x => x.Longitude).HasPrecision(10, 7).IsRequired();
        builder.HasIndex(x => x.City);
    }
}

public sealed class SportConfiguration : IEntityTypeConfiguration<Sport>
{
    public void Configure(EntityTypeBuilder<Sport> builder)
    {
        builder.ToTable("Sports");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Slug).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.HasIndex(x => x.Slug).IsUnique();
    }
}

public sealed class FieldConfiguration : IEntityTypeConfiguration<Field>
{
    public void Configure(EntityTypeBuilder<Field> builder)
    {
        builder.ToTable("Fields");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(2000);
        builder.Property(x => x.Address).HasMaxLength(500).IsRequired();
        builder.Property(x => x.City).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Latitude).HasPrecision(10, 7).IsRequired();
        builder.Property(x => x.Longitude).HasPrecision(10, 7).IsRequired();
        builder.Property(x => x.DayPricePerHour).HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.NightPricePerHour).HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.AverageRating).HasPrecision(3, 2);
        builder.Property(x => x.IsApproved).HasDefaultValue(true);
        builder.HasIndex(x => x.City);
        builder.HasIndex(x => x.SportId);
        builder.HasIndex(x => x.LocationId);
        builder.HasIndex(x => x.OwnerId);
        builder.HasIndex(x => x.IsApproved);
        builder.HasOne(x => x.Sport).WithMany(s => s.Fields).HasForeignKey(x => x.SportId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Location).WithMany(l => l.Fields).HasForeignKey(x => x.LocationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Owner).WithMany(u => u.OwnedFields).HasForeignKey(x => x.OwnerId).OnDelete(DeleteBehavior.SetNull);
    }
}

public sealed class FacilityConfiguration : IEntityTypeConfiguration<Facility>
{
    public void Configure(EntityTypeBuilder<Facility> builder)
    {
        builder.ToTable("Facilities");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Icon).HasMaxLength(50);
        builder.HasIndex(x => x.Name).IsUnique();
    }
}

public sealed class FieldFacilityConfiguration : IEntityTypeConfiguration<FieldFacility>
{
    public void Configure(EntityTypeBuilder<FieldFacility> builder)
    {
        builder.ToTable("FieldFacilities");
        builder.HasKey(x => new { x.FieldId, x.FacilityId });
        builder.HasOne(x => x.Field).WithMany(f => f.Facilities).HasForeignKey(x => x.FieldId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Facility).WithMany(f => f.Fields).HasForeignKey(x => x.FacilityId).OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notifications");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Title).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Message).HasMaxLength(1000).IsRequired();
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => new { x.UserId, x.IsRead });
        builder.HasOne(x => x.User).WithMany(u => u.Notifications).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Action).HasMaxLength(100).IsRequired();
        builder.Property(x => x.EntityName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.EntityId).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Details).HasMaxLength(1000);
        builder.HasIndex(x => x.CreatedAtUtc);
    }
}

public sealed class FieldImageConfiguration : IEntityTypeConfiguration<FieldImage>
{
    public void Configure(EntityTypeBuilder<FieldImage> builder)
    {
        builder.ToTable("FieldImages");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ImageUrl).HasMaxLength(1000).IsRequired();
        builder.HasOne(x => x.Field).WithMany(f => f.Images).HasForeignKey(x => x.FieldId).OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class FieldAmenityConfiguration : IEntityTypeConfiguration<FieldAmenity>
{
    public void Configure(EntityTypeBuilder<FieldAmenity> builder)
    {
        builder.ToTable("FieldAmenities");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Icon).HasMaxLength(50);
        builder.HasOne(x => x.Field).WithMany(f => f.Amenities).HasForeignKey(x => x.FieldId).OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class FieldAvailabilityConfiguration : IEntityTypeConfiguration<FieldAvailability>
{
    public void Configure(EntityTypeBuilder<FieldAvailability> builder)
    {
        builder.ToTable("FieldAvailabilities");
        builder.HasKey(x => x.Id);
        builder.HasOne(x => x.Field).WithMany(f => f.Availability).HasForeignKey(x => x.FieldId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(x => new { x.FieldId, x.Date }).IsUnique();
    }
}

public sealed class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.ToTable("Bookings");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.TotalPrice).HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.ConcurrencyStamp).IsRowVersion();
        builder.HasIndex(x => new { x.FieldId, x.BookingDate });
        builder.HasIndex(x => x.UserId);
        builder.HasOne(x => x.User).WithMany(u => u.Bookings).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Field).WithMany(f => f.Bookings).HasForeignKey(x => x.FieldId).OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.ToTable("Reviews");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Rating).IsRequired();
        builder.Property(x => x.Comment).HasMaxLength(1000);
        builder.HasIndex(x => x.BookingId).IsUnique();
        builder.HasIndex(x => x.FieldId);
        builder.HasOne(x => x.Booking).WithOne(b => b.Review).HasForeignKey<Review>(x => x.BookingId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.User).WithMany(u => u.Reviews).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Field).WithMany(f => f.Reviews).HasForeignKey(x => x.FieldId).OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class FavoriteConfiguration : IEntityTypeConfiguration<Favorite>
{
    public void Configure(EntityTypeBuilder<Favorite> builder)
    {
        builder.ToTable("Favorites");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new { x.UserId, x.FieldId }).IsUnique();
        builder.HasOne(x => x.User).WithMany(u => u.Favorites).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Field).WithMany(f => f.Favorites).HasForeignKey(x => x.FieldId).OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.TokenHash).HasMaxLength(64).IsRequired();
        builder.HasIndex(x => x.TokenHash).IsUnique();
        builder.HasIndex(x => x.UserId);
        builder.HasOne(x => x.User).WithMany(u => u.RefreshTokens).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Amount).HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.Provider).HasMaxLength(50);
        builder.Property(x => x.TransactionId).HasMaxLength(100);
        builder.Property(x => x.FailureReason).HasMaxLength(500);
        builder.HasIndex(x => x.BookingId);
        builder.HasIndex(x => x.TransactionId).IsUnique();
        builder.HasOne(x => x.Booking).WithMany(b => b.Payments).HasForeignKey(x => x.BookingId).OnDelete(DeleteBehavior.Restrict);
    }
}