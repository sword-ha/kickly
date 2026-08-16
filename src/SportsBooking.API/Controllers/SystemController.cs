using Microsoft.AspNetCore.Mvc;

namespace SportsBooking.API.Controllers;

[ApiController]
[Route("api/system")]
public sealed class SystemController : ControllerBase
{
    [HttpGet("health")]
    public ActionResult<object> Health()
        => Ok(new
        {
            Status = "Healthy",
            TimeUtc = DateTime.UtcNow,
            Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"
        });
}
