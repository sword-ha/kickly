namespace SportsBooking.Application.DTOs;

public sealed record LocationDto(int Id, string Name, string City, string Governorate, string Address, decimal Latitude, decimal Longitude);

public sealed record LocationDetailsDto(
    int Id,
    string Name,
    string City,
    string Governorate,
    string Address,
    decimal Latitude,
    decimal Longitude,
    int FieldsCount);

public sealed record CreateLocationRequest(string Name, string City, string Governorate, string Address, decimal Latitude, decimal Longitude);

public sealed record AdminUpdateLocationRequest(string Name, string City, string Governorate, string Address, decimal Latitude, decimal Longitude);
