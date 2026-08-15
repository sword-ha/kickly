namespace SportsBooking.Application.Options;

public sealed class AppOptions
{
    public const string SectionName = "App";

    public string ClientBaseUrl { get; set; } = "https://localhost:55814";
    public string ConfirmEmailPath { get; set; } = "/confirm-email";
    public string ResetPasswordPath { get; set; } = "/reset-password";
}
