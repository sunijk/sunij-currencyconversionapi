using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using CurrencyConversion.Infrastructure;
using Polly;
using Polly.Extensions.Http;
using CurrencyConversion.Infrastructure.Interface;
using CurrencyConversion.Infrastructure.Repositories;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using CurrencyConversion.API.Framework;
using OpenTelemetry.Trace;
using OpenTelemetry.Resources;
using OpenTelemetry.Metrics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using CurrencyConversion.Infrastructure.Interfaces;
using CurrencyConversion.Infrastructure.Provider;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Components.Server.Circuits;

var builder = WebApplication.CreateBuilder(args);

// Add API versioning 
builder.Services.AddApiVersioning(options =>
{
    options.ReportApiVersions = true;
options.AssumeDefaultVersionWhenUnspecified = true;
options.DefaultApiVersion = new ApiVersion(1, 0);
});

builder.Services.AddMemoryCache();
builder.Services.AddLogging();
builder.Services.AddControllers();



// Add HttpClient with resilience policies (Polly)
//Retry policies with exponential backoff to handle intermittent API failures.
// Circuit breaker to  handle API outages.
builder.Services.AddHttpClient<ICurrencyExchangeRateservice, ExchangeRateService>()
.AddPolicyHandler(HttpPolicyExtensions.HandleTransientHttpError()
.WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))))
.AddPolicyHandler(HttpPolicyExtensions.HandleTransientHttpError()
.CircuitBreakerAsync(3, TimeSpan.FromSeconds(30)));

// Register HttpClient for FrankfurterCurrencyProvider

builder.Services.AddHttpClient<FrankfurterCurrencyProvider>()
    .AddPolicyHandler(HttpPolicyExtensions.HandleTransientHttpError()
    .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))))
    .AddPolicyHandler(HttpPolicyExtensions.HandleTransientHttpError()
    .CircuitBreakerAsync(3, TimeSpan.FromSeconds(30)));

// Register providers and factory
builder.Services.AddTransient<ICurrencyProvider, FrankfurterCurrencyProvider>();
builder.Services.AddSingleton<ICurrencyProviderFactory, CurrencyProviderFactory>();


builder.Services.AddEndpointsApiExplorer();

// JWT Configuration
var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];

// Add JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

// ExcludedCurrencies configuration 
var excludedCurrencies = builder.Configuration.GetSection("ExcludedCurrencies").Get<List<string>>();
builder.Services.AddSingleton(excludedCurrencies);

builder.Services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog());
builder.Services.AddAuthorization();
// Swagger configuration
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Currency Conversion API", Version = "v1" });

    // Enable JWT Authentication in Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Enter 'Bearer {your_token}'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            new List<string>()
        }
    });
});

// OpenTelemetry Setup
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("CurrencyConversionAPI"))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddConsoleExporter() // Export traces to console
    )
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddMeter("Microsoft.AspNetCore.Hosting", "Microsoft.AspNetCore.Server.Kestrel")
        .AddConsoleExporter() // Export metrics to console
    );

// Ensure logging is set up correctly
builder.Logging.AddOpenTelemetry(logging =>
{
    logging.IncludeFormattedMessage = true;
    logging.IncludeScopes = true;
    logging.ParseStateValues = true;
});


// Serilog Configuration in Program.cs
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    //.WriteTo.Seq("http://localhost:5341") // Optional: Seq logging
    .CreateLogger();

builder.Host.UseSerilog();

// Write log to Elastic search
//Log.Logger = new LoggerConfiguration()
//    .WriteTo.Console()
//    .WriteTo.Seq("http://localhost:5341")
//    .CreateLogger();

var app = builder.Build();
app.UseMiddleware<LoggingMiddleware>();
app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();

public partial class Program { }