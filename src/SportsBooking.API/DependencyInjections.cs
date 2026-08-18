using Microsoft.AspNetCore.Identity;
using Microsoft.OpenApi;
using SportsBooking.API.OpenApiTransformers;

namespace SportsBooking.API;

public static class DependencyInjections
{
    extension( IServiceCollection services )
    {

        public void AddDependencies(IConfiguration config)
        {

            services.AddEndpointsApiExplorer();
            
            services.AddControllers();

            services.AddValidatorsFromAssemblyContaining<Program>().AddFluentValidationAutoValidation();

            services.AddAuthorization();

            services.AddExceptionHandler<GlobalExceptionHandler>();

            services.AddProblemDetails();


            services.AddOpenApiConfigurations();

            services.AddCorsOnFire();

            services.AddApplicationServices( config );

            services.AddInfrastructureServices( config );

            services.AddAuthentication( config );
        }


        public void AddCorsOnFire() =>
             services.AddCors(s => s.AddDefaultPolicy( s => s.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()) );

        public void AddAuthentication( IConfiguration config )
        {

            services.AddAuthentication( 
            x => 
                {
                    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                }         
            )
             .AddJwtBearer( options =>
             {
                 var jwt = config.GetSection( JwtOptions.SectionName );

                 options.TokenValidationParameters = new TokenValidationParameters
                 {
                     ValidateIssuer = true,
                     ValidateAudience = true,
                     ValidateIssuerSigningKey = true,
                     ValidateLifetime = true,
                     ValidIssuer = jwt["Issuer"],
                     ValidAudience = jwt["Audience"],
                     IssuerSigningKey = new SymmetricSecurityKey( Encoding.UTF8.GetBytes( jwt["Key"] ?? string.Empty ) ),
                     ClockSkew = TimeSpan.Zero
                 };
             } );


            services.Configure<IdentityOptions>( x => 
            { 
                x.SignIn.RequireConfirmedEmail = true;

            } );

        }

        public void AddOpenApiConfigurations()
        {
            services.AddOpenApi( "v1", options =>
            {
                options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();

                options.OpenApiVersion = OpenApiSpecVersion.OpenApi3_0;

                options.AddDocumentTransformer( new ApiVersionTransformer(  ) );
            } );

        }


    }
}
