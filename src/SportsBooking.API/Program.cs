
var builder = WebApplication.CreateBuilder( args );

builder.Services.AddDependencies( builder.Configuration );


var app = builder.Build();


app.MapOpenApi();

if(app.Environment.IsDevelopment())
{
    app.UseSwaggerUI( s => s.SwaggerEndpoint( "/openapi/v1.json", "v1" ) );
}

app.UseExceptionHandler();

app.UseHttpsRedirection();

app.UseCors();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();