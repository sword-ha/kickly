using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SportsBooking.Domain.Entities;

namespace SportsBooking.Infrastructure.Persistence;

public sealed class SportsBookingDbContext : IdentityDbContext<User, IdentityRole<int>, int>
{
    public SportsBookingDbContext(DbContextOptions<SportsBookingDbContext> options)
        : base(options)
    {
    }

    public DbSet<Location> Locations => Set<Location>();
    public DbSet<Sport> Sports => Set<Sport>();
    public DbSet<Facility> Facilities => Set<Facility>();
    public DbSet<FieldFacility> FieldFacilities => Set<FieldFacility>();
    public DbSet<Field> Fields => Set<Field>();
    public DbSet<FieldImage> FieldImages => Set<FieldImage>();
    public DbSet<FieldAmenity> FieldAmenities => Set<FieldAmenity>();
    public DbSet<FieldAvailability> FieldAvailabilities => Set<FieldAvailability>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<Favorite> Favorites => Set<Favorite>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SportsBookingDbContext).Assembly);
    }
}