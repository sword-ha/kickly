using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder( args );

builder.Services.AddDependencies( builder.Configuration );

builder.Services.Configure<ForwardedHeadersOptions>( options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
} );


var app = builder.Build();

app.UseForwardedHeaders();

app.MapOpenApi();


app.UseSwaggerUI( s => s.SwaggerEndpoint( "/openapi/v1.json", "v1" ) );

app.UseExceptionHandler();

if ( !app.Environment.IsProduction() )
{
    app.UseHttpsRedirection();
}

app.UseCors();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.MapGet("/health", () => Results.Ok(new { status = "healthy" }))
    .AllowAnonymous();

app.Run();