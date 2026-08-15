using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SportsBooking.Domain.Entities;
using SportsBooking.Domain.Enums;
using SportsBooking.Infrastructure.Persistence;

namespace SportsBooking.Infrastructure.Seeding;

public static class DevelopmentDataSeeder
{
    public static async Task SeedAsync(
        SportsBookingDbContext db,
        UserManager<User> userManager,
        RoleManager<IdentityRole<int>> roleManager,
        CancellationToken ct = default)
    {
        await db.Database.MigrateAsync(ct);

        // Identity roles
        foreach (var roleName in new[] { UserRole.Customer.ToString(), UserRole.Owner.ToString(), UserRole.Admin.ToString() })
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var createResult = await roleManager.CreateAsync(new IdentityRole<int>(roleName));
                if (!createResult.Succeeded)
                {
                    throw new InvalidOperationException($"Failed to seed role {roleName}: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
                }
            }
        }

        // Seed users
        var admin = await EnsureUserAsync(db, userManager, "Admin", "Admin", "admin@example.com", "01000000001", UserRole.Admin, "Admin@123", ct);
        var owner = await EnsureUserAsync(db, userManager, "Field", "Owner", "owner@example.com", "01000000002", UserRole.Owner, "Owner@123", ct);
        var customer = await EnsureUserAsync(db, userManager, "Demo", "Customer", "customer@example.com", "01000000003", UserRole.Customer, "Customer@123", ct);
        _ = customer;

        if (await db.Sports.AnyAsync(ct))
        {
            return;
        }

        // Sports
        var sports = new List<Sport>
        {
            new() { Type = SportType.Football, Name = "Football", Slug = "football", Description = "5-a-side and 7-a-side football fields." },
            new() { Type = SportType.Padel, Name = "Padel", Slug = "padel", Description = "Padel courts." },
            new() { Type = SportType.Basketball, Name = "Basketball", Slug = "basketball", Description = "Basketball courts." },
            new() { Type = SportType.Tennis, Name = "Tennis", Slug = "tennis", Description = "Tennis courts." },
            new() { Type = SportType.Handball, Name = "Handball", Slug = "handball", Description = "Handball courts." }
        };
        db.Sports.AddRange(sports);
        await db.SaveChangesAsync(ct);

        // Locations
        var locations = new List<Location>
        {
            new() { Name = "Nasr City", City = "Cairo", Governorate = "Cairo", Address = "Nasr City, Cairo", Latitude = 30.049m, Longitude = 31.240m },
            new() { Name = "New Cairo", City = "Cairo", Governorate = "Cairo", Address = "New Cairo, Cairo", Latitude = 30.030m, Longitude = 31.470m },
            new() { Name = "Zamalek", City = "Cairo", Governorate = "Cairo", Address = "Zamalek, Cairo", Latitude = 30.060m, Longitude = 31.220m },
            new() { Name = "Dokki", City = "Giza", Governorate = "Giza", Address = "Dokki, Giza", Latitude = 30.037m, Longitude = 31.210m },
            new() { Name = "Sheikh Zayed", City = "Giza", Governorate = "Giza", Address = "Sheikh Zayed, Giza", Latitude = 30.050m, Longitude = 30.950m },
            new() { Name = "6th of October", City = "Giza", Governorate = "Giza", Address = "6th of October City, Giza", Latitude = 29.940m, Longitude = 30.920m },
            new() { Name = "Mansoura Center", City = "Mansoura", Governorate = "Dakahlia", Address = "Mansoura, Dakahlia", Latitude = 31.040m, Longitude = 31.380m },
            new() { Name = "Mansoura East", City = "Mansoura", Governorate = "Dakahlia", Address = "Mansoura East, Dakahlia", Latitude = 31.050m, Longitude = 31.400m },
            new() { Name = "Smouha", City = "Alexandria", Governorate = "Alexandria", Address = "Smouha, Alexandria", Latitude = 31.200m, Longitude = 29.950m },
            new() { Name = "Sidi Gaber", City = "Alexandria", Governorate = "Alexandria", Address = "Sidi Gaber, Alexandria", Latitude = 31.220m, Longitude = 29.960m }
        };
        db.Locations.AddRange(locations);
        await db.SaveChangesAsync(ct);

        // Facilities
        var facilities = new List<Facility>
        {
            new() { Name = "Floodlights", Icon = "light", IsActive = true },
            new() { Name = "Changing Rooms", Icon = "locker", IsActive = true },
            new() { Name = "Parking", Icon = "car", IsActive = true },
            new() { Name = "Air Conditioning", Icon = "snow", IsActive = true },
            new() { Name = "Pro Shop", Icon = "shop", IsActive = true },
            new() { Name = "Cafeteria", Icon = "coffee", IsActive = true },
            new() { Name = "Security", Icon = "shield", IsActive = true },
            new() { Name = "First Aid", Icon = "medkit", IsActive = true }
        };
        db.Facilities.AddRange(facilities);
        await db.SaveChangesAsync(ct);

        // Fields
        var fields = new List<Field>
        {
            new()
            {
                OwnerId = owner.Id, SportId = sports[0].Id, LocationId = locations[0].Id, Name = "Nasr City Football Arena",
                Description = "Premium 5-a-side football field with artificial turf.",
                Address = "Nasr City, Cairo", City = "Cairo", FieldType = FieldType.Outdoor,
                Latitude = 30.049m, Longitude = 31.240m, DayPricePerHour = 300m, NightPricePerHour = 450m,
                IsApproved = true, ApprovedAtUtc = DateTime.UtcNow
            },
            new()
            {
                OwnerId = owner.Id, SportId = sports[0].Id, LocationId = locations[1].Id, Name = "New Cairo Football Hub",
                Description = "Modern 7-a-side football field with floodlights.",
                Address = "New Cairo, Cairo", City = "Cairo", FieldType = FieldType.Outdoor,
                Latitude = 30.030m, Longitude = 31.470m, DayPricePerHour = 350m, NightPricePerHour = 500m,
                IsApproved = true, ApprovedAtUtc = DateTime.UtcNow
            },
            new()
            {
                OwnerId = owner.Id, SportId = sports[1].Id, LocationId = locations[2].Id, Name = "Zamalek Padel Club",
                Description = "Indoor padel courts with professional surfaces.",
                Address = "Zamalek, Cairo", City = "Cairo", FieldType = FieldType.Indoor,
                Latitude = 30.060m, Longitude = 31.220m, DayPricePerHour = 400m, NightPricePerHour = 550m,
                IsApproved = true, ApprovedAtUtc = DateTime.UtcNow
            },
            new()
            {
                OwnerId = owner.Id, SportId = sports[2].Id, LocationId = locations[3].Id, Name = "Dokki Basketball Court",
                Description = "Outdoor basketball court with high-quality flooring.",
                Address = "Dokki, Giza", City = "Giza", FieldType = FieldType.Outdoor,
                Latitude = 30.037m, Longitude = 31.210m, DayPricePerHour = 200m, NightPricePerHour = 300m,
                IsApproved = true, ApprovedAtUtc = DateTime.UtcNow
            },
            new()
            {
                OwnerId = owner.Id, SportId = sports[3].Id, LocationId = locations[4].Id, Name = "Sheikh Zayed Tennis Courts",
                Description = "Clay tennis courts with night lighting.",
                Address = "Sheikh Zayed, Giza", City = "Giza", FieldType = FieldType.Outdoor,
                Latitude = 30.050m, Longitude = 30.950m, DayPricePerHour = 500m, NightPricePerHour = 700m,
                IsApproved = true, ApprovedAtUtc = DateTime.UtcNow
            },
            new()
            {
                OwnerId = owner.Id, SportId = sports[4].Id, LocationId = locations[5].Id, Name = "October Handball Arena",
                Description = "Indoor handball court with professional flooring.",
                Address = "6th of October City, Giza", City = "Giza", FieldType = FieldType.Indoor,
                Latitude = 29.940m, Longitude = 30.920m, DayPricePerHour = 400m, NightPricePerHour = 600m,
                IsApproved = true, ApprovedAtUtc = DateTime.UtcNow
            },
            new()
            {
                OwnerId = owner.Id, SportId = sports[0].Id, LocationId = locations[6].Id, Name = "Mansoura Football Field",
                Description = "Community football field in central Mansoura.",
                Address = "Mansoura, Dakahlia", City = "Mansoura", FieldType = FieldType.Outdoor,
                Latitude = 31.040m, Longitude = 31.380m, DayPricePerHour = 150m, NightPricePerHour = 250m,
                IsApproved = true, ApprovedAtUtc = DateTime.UtcNow
            },
            new()
            {
                OwnerId = owner.Id, SportId = sports[1].Id, LocationId = locations[7].Id, Name = "Mansoura Padel Center",
                Description = "Modern padel courts in East Mansoura.",
                Address = "Mansoura East, Dakahlia", City = "Mansoura", FieldType = FieldType.Indoor,
                Latitude = 31.050m, Longitude = 31.400m, DayPricePerHour = 250m, NightPricePerHour = 350m,
                IsApproved = true, ApprovedAtUtc = DateTime.UtcNow
            },
            new()
            {
                OwnerId = owner.Id, SportId = sports[0].Id, LocationId = locations[8].Id, Name = "Smouha Football Park",
                Description = "5-a-side football field in Smouha, Alexandria.",
                Address = "Smouha, Alexandria", City = "Alexandria", FieldType = FieldType.Outdoor,
                Latitude = 31.200m, Longitude = 29.950m, DayPricePerHour = 300m, NightPricePerHour = 450m,
                IsApproved = true, ApprovedAtUtc = DateTime.UtcNow
            },
            new()
            {
                OwnerId = owner.Id, SportId = sports[3].Id, LocationId = locations[9].Id, Name = "Sidi Gaber Tennis Club",
                Description = "Hard tennis courts in Sidi Gaber, Alexandria.",
                Address = "Sidi Gaber, Alexandria", City = "Alexandria", FieldType = FieldType.Outdoor,
                Latitude = 31.220m, Longitude = 29.960m, DayPricePerHour = 450m, NightPricePerHour = 650m,
                IsApproved = true, ApprovedAtUtc = DateTime.UtcNow
            }
        };
        db.Fields.AddRange(fields);
        await db.SaveChangesAsync(ct);

        // Images
        var images = new List<FieldImage>();
        foreach (var field in fields)
        {
            images.Add(new FieldImage { FieldId = field.Id, ImageUrl = $"https://example.com/fields/{field.Id}/main.jpg", DisplayOrder = 1, IsPrimary = true });
            images.Add(new FieldImage { FieldId = field.Id, ImageUrl = $"https://example.com/fields/{field.Id}/2.jpg", DisplayOrder = 2, IsPrimary = false });
        }
        db.FieldImages.AddRange(images);

        // Amenities
        var amenities = new List<FieldAmenity>
        {
            new() { FieldId = fields[0].Id, Name = "Floodlights", Icon = "light" },
            new() { FieldId = fields[0].Id, Name = "Changing Rooms", Icon = "locker" },
            new() { FieldId = fields[1].Id, Name = "Parking", Icon = "car" },
            new() { FieldId = fields[2].Id, Name = "Air Conditioning", Icon = "snow" },
            new() { FieldId = fields[4].Id, Name = "Pro Shop", Icon = "shop" },
            new() { FieldId = fields[8].Id, Name = "Cafeteria", Icon = "coffee" }
        };
        db.FieldAmenities.AddRange(amenities);

        // Field facilities
        var fieldFacilities = new List<FieldFacility>
        {
            new() { FieldId = fields[0].Id, FacilityId = facilities[0].Id },
            new() { FieldId = fields[0].Id, FacilityId = facilities[1].Id },
            new() { FieldId = fields[1].Id, FacilityId = facilities[2].Id },
            new() { FieldId = fields[2].Id, FacilityId = facilities[3].Id },
            new() { FieldId = fields[4].Id, FacilityId = facilities[4].Id },
            new() { FieldId = fields[8].Id, FacilityId = facilities[5].Id }
        };
        db.FieldFacilities.AddRange(fieldFacilities);

        // Availability for the next 7 days
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var availabilities = new List<FieldAvailability>();
        foreach (var field in fields)
        {
            for (var i = 0; i < 7; i++)
            {
                availabilities.Add(new FieldAvailability
                {
                    FieldId = field.Id,
                    Date = today.AddDays(i),
                    OpenTime = new TimeOnly(8, 0),
                    CloseTime = new TimeOnly(23, 0),
                    IsClosed = false
                });
            }
        }
        db.FieldAvailabilities.AddRange(availabilities);

        await db.SaveChangesAsync(ct);
    }

    private static async Task<User> EnsureUserAsync(
        SportsBookingDbContext db,
        UserManager<User> userManager,
        string firstName,
        string lastName,
        string email,
        string phoneNumber,
        UserRole role,
        string password,
        CancellationToken ct)
    {
        var existing = await db.Users.FirstOrDefaultAsync(u => u.Email == email, ct);
        if (existing is not null)
        {
            return existing;
        }

        var user = new User
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            UserName = email,
            PhoneNumber = phoneNumber,
            Role = role,
            IsActive = true,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException($"Failed to seed user {email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }

        var roleResult = await userManager.AddToRoleAsync(user, role.ToString());
        if (!roleResult.Succeeded)
        {
            throw new InvalidOperationException($"Failed to assign role {role} to {email}.");
        }

        return user;
    }
}
