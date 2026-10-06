using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Vinsaro.Application.Extensions;

/// <summary>
/// Registers all application-layer services into the DI container.
/// Called from Vinsaro.Api's Program.cs.
/// </summary>
public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Register all FluentValidation validators from this assembly
        services.AddValidatorsFromAssembly(typeof(ApplicationServiceExtensions).Assembly);

        return services;
    }
}
