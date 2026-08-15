using System.Security.Claims;

namespace SportsBooking.API.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static int GetRequiredUserId(this ClaimsPrincipal principal)
    {
        var value = principal.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? principal.FindFirstValue("sub");

        if (int.TryParse(value, out var userId))
        {
            return userId;
        }

        throw new UnauthorizedAccessException("User id claim was not found.");
    }
}