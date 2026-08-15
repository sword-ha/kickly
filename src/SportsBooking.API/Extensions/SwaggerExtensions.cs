namespace SportsBooking.API.Extensions;

public static class SwaggerExtensions
{
    public static IServiceCollection AddSwaggerDocumentation(
        this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new Microsoft.OpenApi.OpenApiInfo
            {
                Title = "Sports Booking API",
                Version = "v1",
                Description = "REST API for a Sports Field Booking application."
            });

            options.CustomSchemaIds(t => t.FullName);
        });

        return services;
    }
}