using CurrencyWalletSystem.API.Authorization;
using CurrencyWalletSystem.Gateway.Interfaces;
using CurrencyWalletSystem.Gateway.Services;
using CurrencyWalletSystem.Infrastructure.Data;
using CurrencyWalletSystem.Infrastructure.Factories;
using CurrencyWalletSystem.Infrastructure.Interfaces;
using CurrencyWalletSystem.Infrastructure.Services;
using CurrencyWalletSystem.Infrastructure.Strategies;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Diagnostics.CodeAnalysis;
using System.Threading.RateLimiting;

[ExcludeFromCodeCoverage]
public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        ConfigureServices(builder);

        var app = builder.Build();

        ConfigureApp(app);

        app.Run();
    }

    /// <summary>
    /// Configures services required for the application.
    /// </summary>
    /// <param name="builder">The builder for configuring services.</param>
    private static void ConfigureServices(WebApplicationBuilder builder)
    {
        builder.Services.AddControllers(options =>
        {
            options.Filters.Add<ApiKeyAuthorizationFilter>();
        });
        builder.Services.AddMemoryCache();

        ConfigureDbContext(builder);
        ConfigureHttpClient(builder);

        ConfigureStrategies(builder);
        ConfigureWalletServices(builder);

        ConfigureRateLimiter(builder);

        ConfigureSwagger(builder);
    }

    /// <summary>
    /// Configures the database context.
    /// </summary>
    /// <param name="builder">The builder for configuring services.</param>
    private static void ConfigureDbContext(WebApplicationBuilder builder)
    {
        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
    }

    /// <summary>
    /// Configures the HTTP client for ECB exchange rate provider.
    /// </summary>
    /// <param name="builder">The builder for configuring services.</param>
    private static void ConfigureHttpClient(WebApplicationBuilder builder)
    {
        builder.Services.AddHttpClient<IEcbExchangeRateProvider, EcbExchangeRateProvider>();
    }

    /// <summary>
    /// Configures strategies related to wallet operations.
    /// </summary>
    /// <param name="builder">The builder for configuring services.</param>
    private static void ConfigureStrategies(WebApplicationBuilder builder)
    {
        builder.Services.AddTransient<AddFundsStrategy>();
        builder.Services.AddTransient<SubtractFundsStrategy>();
        builder.Services.AddTransient<ForceSubtractFundsStrategy>();
    }

    /// <summary>
    /// Configures wallet-related services.
    /// </summary>
    /// <param name="builder">The builder for configuring services.</param>
    private static void ConfigureWalletServices(WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<IWalletStrategyFactory, WalletStrategyFactory>();
        builder.Services.AddScoped<IWalletService, WalletService>();
        builder.Services.AddScoped<ICurrencyRateCache, CurrencyRateCache>();
    }

    /// <summary>
    /// Configures a client IP-based rate limiter for the application.
    /// </summary>
    /// <param name="builder">The builder used to configure services.</param>
    private static void ConfigureRateLimiter(WebApplicationBuilder builder)
    {
        builder.Services.AddRateLimiter(options =>
        {
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
            {
                var clientIp = GetClientIp(httpContext);

                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: clientIp,
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = 10,
                        QueueLimit = 0,
                        Window = TimeSpan.FromSeconds(60)
                    });
            });

            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.OnRejected = async (context, cancellationToken) =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogWarning("Rate limit exceeded for user {User}.", context.HttpContext.User.Identity?.Name ?? "Unknown");

                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.HttpContext.Response.Headers["Retry-After"] = "60";

                await context.HttpContext.Response.WriteAsync("Rate limit exceeded. Please try again later.", cancellationToken);
            };
        });
    }

    /// <summary>
    /// Retrieves the client IP address from the request, supporting common proxy headers.
    /// </summary>
    /// <param name="context">The current HTTP context.</param>
    /// <returns>The client IP address as a string.</returns>
    private static string GetClientIp(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor))
        {
            return forwardedFor.FirstOrDefault() ?? context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }

        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }

    /// <summary>
    /// Configures Swagger for API documentation.
    /// </summary>
    /// <param name="builder">The builder for configuring services.</param>
    private static void ConfigureSwagger(WebApplicationBuilder builder)
    {
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "CurrencyWalletSystem API", Version = "v1" });
            options.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Name = "x-api-key",
                Type = SecuritySchemeType.ApiKey,
                Description = "API Key needed to access endpoints"
            });
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "ApiKey" }
                        },
                        Array.Empty<string>()
                    }
                });
        });
    }

    /// <summary>
    /// Configures middleware and request pipeline for the application.
    /// </summary>
    /// <param name="app">The application builder.</param>
    private static void ConfigureApp(WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "CurrencyWalletSystem API v1"));
        }

        app.UseRateLimiter();
        app.UseHttpsRedirection();
        app.MapControllers();
    }
}