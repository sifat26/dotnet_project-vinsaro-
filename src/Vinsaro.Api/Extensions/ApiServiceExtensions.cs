using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Vinsaro.Api.Middleware;

namespace Vinsaro.Api.Extensions;

/// <summary>
/// Registers API-layer services: JWT, Swagger, CORS, rate limiting, and health checks.
/// </summary>
public static class ApiServiceExtensions
{
    public static IServiceCollection AddApiServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ── JWT Authentication ────────────────────────────────────────────────
        var jwtSection = configuration.GetSection("Jwt");
        var jwtKey = jwtSection["Key"]
            ?? throw new InvalidOperationException("JWT Key is missing from configuration.");

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSection["Issuer"],
                ValidAudience = jwtSection["Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                ClockSkew = TimeSpan.FromSeconds(30)
            };
        });

        services.AddAuthorization();

        // ── CORS ─────────────────────────────────────────────────────────────
        // Angular frontend origin(s) will be read from configuration.
        // Unrestricted CORS is NOT allowed in Production.
        services.AddCors(options =>
        {
            options.AddPolicy("AngularFrontend", policy =>
            {
                var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                    ?? ["http://localhost:4200"];

                policy.WithOrigins(allowedOrigins)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            });
        });

        // ── Controllers ───────────────────────────────────────────────────────
        services.AddControllers();

        // ── Swagger / OpenAPI ─────────────────────────────────────────────────
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Vinsaro API",
                Version = "v1",
                Description = "Backend API for Vinsaro — a Bangladeshi fashion e-commerce platform.",
                Contact = new OpenApiContact { Name = "Vinsaro Team" }
            });

            // JWT bearer security definition for Swagger UI
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter your JWT token. Example: Bearer {token}"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });

            // Include XML documentation comments if available
            var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
                options.IncludeXmlComments(xmlPath);
        });

        // ── Health Checks ─────────────────────────────────────────────────────
        services.AddHealthChecks();

        // ── Rate Limiting ─────────────────────────────────────────────────────
        services.AddRateLimiter(options =>
        {
            // Fixed window: 10 login/register attempts per minute per IP
            options.AddFixedWindowLimiter("auth", limiter =>
            {
                limiter.PermitLimit = 10;
                limiter.Window = TimeSpan.FromMinutes(1);
                limiter.QueueLimit = 0;
            });

            // General API: 100 requests per minute per IP
            options.AddFixedWindowLimiter("api", limiter =>
            {
                limiter.PermitLimit = 100;
                limiter.Window = TimeSpan.FromMinutes(1);
                limiter.QueueLimit = 5;
            });

            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        });

        // ── Global Exception Handling ─────────────────────────────────────────
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        return services;
    }
}
