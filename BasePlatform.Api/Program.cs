using BasePlatform.Api.DependencyInjection;
using BasePlatform.Api.Middleware;
using BasePlatform.Infrastructure.DependencyInjection;
using BasePlatform.Infrastructure.Identity;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, config) =>
        config.ReadFrom.Configuration(context.Configuration)
              .ReadFrom.Services(services)
              .Enrich.FromLogContext()
              .Enrich.WithMachineName()
              .Enrich.WithEnvironmentName());

    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddApiServices(builder.Configuration);
    builder.Services.AddObservability(builder.Configuration);

    var app = builder.Build();

    using (var scope = app.Services.CreateScope())
    {
        var seeder = scope.ServiceProvider.GetRequiredService<IdentitySeeder>();
        await seeder.SeedAsync();
    }

    app.UseMiddleware<ExceptionHandlingMiddleware>();
    app.UseMiddleware<CorrelationIdMiddleware>();
    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate =
            "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000}ms";
    });

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "BasePlatform API v1");
            options.RoutePrefix = "swagger";
        });
    }

    app.UseHttpsRedirection();

    app.Use(async (context, next) =>
    {
        context.Response.Headers["X-Content-Type-Options"] = "nosniff";
        context.Response.Headers["X-Frame-Options"] = "DENY";
        context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
        context.Response.Headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
        await next();
    });

    app.UseCors("DefaultPolicy");
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();
    app.MapHealthChecks("/health");

    Log.Information("BasePlatform.Api started on {Urls}",
        string.Join(", ", builder.WebHost.GetSetting("urls")?.Split(";") ?? ["http://localhost:5280"]));

    if (app.Environment.IsDevelopment())
    {
        var devOtpDir = Path.Combine(app.Environment.ContentRootPath, "dev-emails");
        Log.Information(
            "Development mode: OTP codes are written to {DevOtpPath} (email, SMS, forgot-password)",
            Path.Combine(devOtpDir, "latest-otp.txt"));
    }

    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly.");
}
finally
{
    Log.CloseAndFlush();
}