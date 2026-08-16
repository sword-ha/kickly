using Microsoft.OpenApi;

namespace SportsBooking.API.Extensions;

public static class SwaggerExtensions
{
    public static IServiceCollection AddSwaggerDocumentation(
        this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Sports Booking API",
                Version = "v1",
                Description = "REST API for a Sports Field Booking application."
            });

            options.CustomSchemaIds(t => t.FullName);

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Paste your JWT access token. Do NOT include the word 'Bearer' — just paste the token."
            });

            options.AddSecurityRequirement(_ => new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecuritySchemeReference("Bearer", null, null),
                    new List<string>()
                }
            });
        });

        return services;
    }
}
