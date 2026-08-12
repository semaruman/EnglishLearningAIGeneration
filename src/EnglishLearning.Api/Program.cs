using EnglishLearning.Api.Extensions;
using EnglishLearning.Api.Middleware;
using EnglishLearning.Application;
using EnglishLearning.Infrastructure;
using EnglishLearning.Infrastructure.Persistence;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.FileProviders;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console());

    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddControllers();
    builder.Services.AddSwaggerWithJwt();

    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
            policy.AllowAnyHeader()
                .AllowAnyMethod()
                .AllowAnyOrigin());
    });

    builder.Services.AddRateLimiter(options =>
    {
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        options.OnRejected = async (context, token) =>
        {
            context.HttpContext.Response.ContentType = "application/json";
            await context.HttpContext.Response.WriteAsJsonAsync(new
            {
                success = false,
                error = new { code = "RATE_LIMITED", message = "Too many requests. Please try again later." }
            }, token);
        };

        options.AddFixedWindowLimiter("practice-generate", limiter =>
        {
            limiter.PermitLimit = 10;
            limiter.Window = TimeSpan.FromMinutes(1);
            limiter.QueueLimit = 0;
        });

        options.AddFixedWindowLimiter("ai-word-add", limiter =>
        {
            limiter.PermitLimit = 20;
            limiter.Window = TimeSpan.FromMinutes(1);
            limiter.QueueLimit = 0;
        });
    });

    var app = builder.Build();

    app.UseMiddleware<ExceptionHandlingMiddleware>();
    app.UseSerilogRequestLogging();

    app.UseSwagger();
    app.UseSwaggerUI();

    // Keep HTTP in Docker; HTTPS redirection is optional for local HTTPS profile
    if (!app.Environment.IsEnvironment("Testing") &&
        !string.Equals(Environment.GetEnvironmentVariable("DISABLE_HTTPS_REDIRECTION"), "true", StringComparison.OrdinalIgnoreCase))
    {
        app.UseHttpsRedirection();
    }
    app.UseCors();

    var frontendPath = ResolveFrontendPath(builder.Environment.ContentRootPath);
    if (frontendPath is not null)
    {
        var fileProvider = new PhysicalFileProvider(frontendPath);
        app.UseDefaultFiles(new DefaultFilesOptions { FileProvider = fileProvider });
        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = fileProvider,
            RequestPath = ""
        });
    }

    app.UseAuthentication();
    app.UseAuthorization();
    app.UseRateLimiter();

    app.MapControllers();

    if (frontendPath is not null)
    {
        var fileProvider = new PhysicalFileProvider(frontendPath);
        app.MapFallbackToFile("index.html", new StaticFileOptions { FileProvider = fileProvider });
    }

    await DatabaseInitializer.InitializeAsync(app.Services);

    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
    throw;
}
finally
{
    await Log.CloseAndFlushAsync();
}

static string? ResolveFrontendPath(string contentRoot)
{
    var candidates = new[]
    {
        Path.GetFullPath(Path.Combine(contentRoot, "frontend")),
        Path.GetFullPath(Path.Combine(contentRoot, "..", "..", "frontend")),
        Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "frontend")),
        Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "frontend"))
    };

    return candidates.FirstOrDefault(Directory.Exists);
}
