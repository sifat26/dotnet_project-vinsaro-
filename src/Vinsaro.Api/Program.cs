using Serilog;
using Serilog.Events;
using Vinsaro.Api.Extensions;
using Vinsaro.Application.Extensions;
using Vinsaro.Infrastructure.Extensions;

// ─────────────────────────────────────────────────────────────────────────────
// Bootstrap Serilog early so startup errors are captured
// ─────────────────────────────────────────────────────────────────────────────
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting Vinsaro API...");

    var builder = WebApplication.CreateBuilder(args);

    // ── Serilog ───────────────────────────────────────────────────────────────
    builder.Host.UseSerilog((context, services, config) =>
        config
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.File(
                path: "logs/vinsaro-.log",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 14,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
            )
    );

    // ── Service Registration ───────────────────────────────────────────────────
    builder.Services
        .AddInfrastructure(builder.Configuration)
        .AddApplication()
        .AddApiServices(builder.Configuration);

    var app = builder.Build();

    // ── HTTP Pipeline ──────────────────────────────────────────────────────────
    app.UseExceptionHandler();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Vinsaro API v1");
            options.RoutePrefix = "swagger";
        });
    }

    app.UseHttpsRedirection();
    app.UseSerilogRequestLogging(opts =>
    {
        opts.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
        // Don't log sensitive headers
        opts.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
            diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"].ToString());
        };
    });

    app.UseCors("AngularFrontend");
    app.UseRateLimiter();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
    app.MapHealthChecks("/health");

    Log.Information("Vinsaro API started on {Urls}", string.Join(", ", builder.Configuration["urls"] ?? "default ports"));

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "Vinsaro API failed to start.");
    throw;
}
finally
{
    Log.CloseAndFlush();
}

// Required for integration testing with WebApplicationFactory
public partial class Program { }
