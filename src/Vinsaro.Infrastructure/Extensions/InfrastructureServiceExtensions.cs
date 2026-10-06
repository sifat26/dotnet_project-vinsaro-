using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Vinsaro.Application.Features.Auth.Interfaces;
using Vinsaro.Domain.Entities;
using Vinsaro.Infrastructure.Identity;
using Vinsaro.Infrastructure.Identity.Options;
using Vinsaro.Infrastructure.Identity.Services;
using Vinsaro.Application.Features.Catalog.Interfaces;
using Vinsaro.Infrastructure.Catalog.Services;
using Vinsaro.Application.Features.Checkout.Interfaces;
using Vinsaro.Infrastructure.Checkout.Services;
using Vinsaro.Application.Features.Payments.Interfaces;
using Vinsaro.Infrastructure.Payments.Services;
using Vinsaro.Application.Features.Coupons.Interfaces;
using Vinsaro.Infrastructure.Coupons.Services;
using Vinsaro.Application.Features.Reviews.Interfaces;
using Vinsaro.Infrastructure.Reviews.Services;
using Vinsaro.Application.Features.Reporting.Interfaces;
using Vinsaro.Infrastructure.Reporting.Services;
using Vinsaro.Infrastructure.Persistence;

namespace Vinsaro.Infrastructure.Extensions;

/// <summary>
/// Registers all infrastructure-layer services into the DI container.
/// Called from Vinsaro.Api's Program.cs.
/// </summary>
public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ── Database ─────────────────────────────────────────────────────────
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' is missing. " +
                "Set it in appsettings.json, environment variables, or user secrets.");

        services.AddDbContext<VinsaroDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsql =>
            {
                npgsql.CommandTimeout(30);
                npgsql.EnableRetryOnFailure(3);
            });
        });

        // ── ASP.NET Core Identity ─────────────────────────────────────────────
        services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
        {
            // Password policy — strong but usable
            options.Password.RequireDigit = true;
            options.Password.RequiredLength = 8;
            options.Password.RequireUppercase = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireNonAlphanumeric = false;

            // Lock out after 5 failed attempts for 15 minutes
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;

            // Use email as the unique username
            options.User.RequireUniqueEmail = true;

            // Require confirmed email in production (toggle via config in future)
            options.SignIn.RequireConfirmedEmail = false;
        })
        .AddEntityFrameworkStores<VinsaroDbContext>()
        .AddDefaultTokenProviders();

        // Options
        services.Configure<JwtOptions>(configuration.GetSection("Jwt"));

        // Services
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ICartService, CartService>();
        services.AddScoped<ICustomerAddressService, CustomerAddressService>();
        services.AddScoped<ICheckoutService, CheckoutService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<ICouponService, CouponService>();
        services.AddScoped<IReviewService, ReviewService>();
        services.AddScoped<IReportService, ReportService>();

        return services;
    }
}
