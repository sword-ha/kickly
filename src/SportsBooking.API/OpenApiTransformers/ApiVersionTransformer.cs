using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace SportsBooking.API.OpenApiTransformers;

public class ApiVersionTransformer( ) : IOpenApiDocumentTransformer
{

    public Task TransformAsync( OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken )
    {

        document.Info = new OpenApiInfo
        {
            Title = "Sports Booking API",
            Version ="v1",
            Description = "REST API for a Sports Field Booking application."
        };

        return Task.CompletedTask;
    }
}
