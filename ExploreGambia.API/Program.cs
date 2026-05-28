using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using ExploreGambia.API.CustomActionFilters;
using ExploreGambia.API.Data;
using ExploreGambia.API.Mapping;
using ExploreGambia.API.Middleware;
using ExploreGambia.API.Models.Domain;
using ExploreGambia.API.Repositories;
using ExploreGambia.API.Services;
using ExploreGambia.API.Services.Payments;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Npgsql;
using Serilog;
using System;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Serialization;
using Stripe;
using ExploreGambia.API.Models.Configurations;


var builder = WebApplication.CreateBuilder(args);

var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(port))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

// Add services to the container.

// Configure Serilog
var logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("Logs/ExploreGambia_Log.txt", rollingInterval: RollingInterval.Minute)
    .MinimumLevel.Information()
    .CreateLogger();

builder.Logging.ClearProviders();
builder.Logging.AddSerilog(logger);


builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidateModelAttribute>(); // Add the global filter here
})
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173",
            "https://eg-frontend-pi.vercel.app")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});


// Add API Versioning
var apiVersioningBuilder = builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;

    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),
        new HeaderApiVersionReader("x-api-version"),
        new QueryStringApiVersionReader("api-version")
    );
});

// Configure API Explorer for versioning
apiVersioningBuilder.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV"; // Example: "v1"
    options.SubstituteApiVersionInUrl = true;
});



// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Explore Gambia API", Version = "v1" });
    options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
    {
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = JwtBearerDefaults.AuthenticationScheme
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = JwtBearerDefaults.AuthenticationScheme
                },
                Scheme = JwtBearerDefaults.AuthenticationScheme,
                Name = JwtBearerDefaults.AuthenticationScheme,
                In = ParameterLocation.Header
            },
            new List<string>()
        }
    });
});

builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<AutoMapperProfiles>();
});

// Add Repositories
builder.Services.AddScoped<ITourRepository, TourRepository>();
builder.Services.AddScoped<ITourGuideRepository, TourGuideRepository>();
builder.Services.AddScoped<IBookingRepository, BookingRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<ITokenRepository, TokenRepository>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IStripePaymentService, StripePaymentService>();

builder.Services.Configure<ModemPayOptions>(options =>
{
    options.PublicKey = Environment.GetEnvironmentVariable("MODEMPAY_PUBLIC_KEY")
        ?? builder.Configuration["ModemPay:PublicKey"]
        ?? string.Empty;
    options.SecretKey = Environment.GetEnvironmentVariable("MODEMPAY_SECRET_KEY")
        ?? builder.Configuration["ModemPay:SecretKey"]
        ?? string.Empty;
    options.WebhookSecret = Environment.GetEnvironmentVariable("MODEMPAY_WEBHOOK_SECRET")
        ?? builder.Configuration["ModemPay:WebhookSecret"]
        ?? string.Empty;
    options.Currency = Environment.GetEnvironmentVariable("MODEMPAY_CURRENCY")
        ?? builder.Configuration["ModemPay:Currency"]
        ?? "GMD";
    options.BaseUrl = Environment.GetEnvironmentVariable("MODEMPAY_BASE_URL")
        ?? builder.Configuration["ModemPay:BaseUrl"]
        ?? "https://api.modempay.com";
    options.TransactionPathTemplate = Environment.GetEnvironmentVariable("MODEMPAY_TRANSACTION_PATH_TEMPLATE")
        ?? builder.Configuration["ModemPay:TransactionPathTemplate"]
        ?? "/transactions/{transactionId}";
});

builder.Services.AddHttpClient<IModemPayClient, ModemPayClient>((serviceProvider, client) =>
{
    var options =
        serviceProvider
            .GetRequiredService<Microsoft.Extensions.Options.IOptions<ModemPayOptions>>()
            .Value;

    client.BaseAddress =
        new Uri(options.BaseUrl);

    client.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue(
            "Bearer",
            options.SecretKey);

    client.DefaultRequestHeaders.Accept.Add(
        new MediaTypeWithQualityHeaderValue("application/json"));
});

// Stripe configuration

builder.Services.Configure<StripeOptions>(options =>
{
    options.SecretKey = Environment.GetEnvironmentVariable("STRIPE_SECRET_KEY")
        ?? builder.Configuration["Stripe:SecretKey"]
        ?? string.Empty;
    options.PublishableKey = Environment.GetEnvironmentVariable("STRIPE_PUBLISHABLE_KEY")
        ?? builder.Configuration["Stripe:PublishableKey"]
        ?? string.Empty;
    options.SuccessUrl = Environment.GetEnvironmentVariable("STRIPE_SUCCESS_URL")
        ?? builder.Configuration["Stripe:SuccessUrl"]
        ?? string.Empty;
    options.CancelUrl = Environment.GetEnvironmentVariable("STRIPE_CANCEL_URL")
        ?? builder.Configuration["Stripe:CancelUrl"]
        ?? string.Empty;
    options.WebhookSecret = Environment.GetEnvironmentVariable("STRIPE_WEBHOOK_SECRET")
        ?? builder.Configuration["Stripe:WebhookSecret"]
        ?? string.Empty;
});

var stripeSecretKey = Environment.GetEnvironmentVariable("STRIPE_SECRET_KEY");
if (string.IsNullOrEmpty(stripeSecretKey))
{
    throw new InvalidOperationException("Stripe Secret key is missing!");
}

StripeConfiguration.ApiKey = stripeSecretKey;



builder.Services.AddIdentityCore<ApplicationUser>()
    .AddRoles<IdentityRole>()
    .AddTokenProvider<DataProtectorTokenProvider<ApplicationUser>>("ExploreGambia")
    .AddEntityFrameworkStores<ExploreGambiaAuthDbContext>()
    .AddDefaultTokenProviders();

// Add AuthService to DI
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 1;
});

builder.Services.AddDataProtection().PersistKeysToDbContext<ExploreGambiaAuthDbContext>();


var dbConnection = ResolvePostgresConnectionString(builder.Configuration);
if (string.IsNullOrWhiteSpace(dbConnection))
{
    throw new InvalidOperationException(
        "PostgreSQL connection is missing. Configure ConnectionStrings__DefaultConnection, DATABASE_URL, or PGHOST/PGPORT/PGDATABASE/PGUSER/PGPASSWORD.");
}

if (!builder.Environment.IsDevelopment() &&
    (dbConnection.Contains("localhost", StringComparison.OrdinalIgnoreCase) ||
     dbConnection.Contains("127.0.0.1", StringComparison.OrdinalIgnoreCase)))
{
    throw new InvalidOperationException(
        "Production database connection is pointing to localhost. Configure ConnectionStrings__DefaultConnection in Railway.");
}

ValidatePostgresConnectionString(dbConnection);

// Add DbContext
builder.Services.AddDbContext<ExploreGambiaDbContext>(options => 
options.UseNpgsql(
    dbConnection, npsql => npsql.MigrationsHistoryTable("__EFMigrationsHistory_App")));
 
builder.Services.AddDbContext<ExploreGambiaAuthDbContext>(options =>
    options.UseNpgsql(
    dbConnection, npsql => npsql.MigrationsHistoryTable("__EFMigrationsHistory_Auth")));

// Read the secret key from environment variable
var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET");

if (string.IsNullOrEmpty(jwtSecret))
{
    throw new InvalidOperationException("JWT Secret key is missing!");
}

// Configure JWT authentication
var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            //ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidAudiences = new[] { builder.Configuration["Jwt:Audience"] },
            IssuerSigningKey = key

        };
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                {
                    Log.Warning("Expired JWT Token.");
                    context.Response.Headers["WWW-Authenticate"] = "Bearer, error=\"expired_token\"";
                }
                else if (context.Exception.GetType() == typeof(SecurityTokenInvalidSignatureException) || context.Exception.GetType() == typeof(SecurityTokenException))
                {
                    Log.Warning("Invalid JWT Token.");
                    context.Response.Headers["WWW-Authenticate"] = "Bearer, error=\"invalid_token\"";
                }
                else
                {
                    Log.Error(context.Exception, "JWT Authentication failed.");
                }
                return Task.CompletedTask;
            },
        };
    });

builder.Services.AddScoped<DataSeeder>();



try
{
    var app = builder.Build();
    
    // Apply migrations and seed data
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var seeder = services.GetRequiredService<DataSeeder>();
        await seeder.SeedAsync();  // ✅ Breakpoint here to see exact error
    }
    
    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseMiddleware<GlobalExceptionHandler>();

    app.UseHttpsRedirection();

    app.UseCors("FrontendPolicy");

    app.UseAuthentication();

    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine($"Fatal error: {ex.Message}\n{ex.StackTrace}");
}

static string? ResolvePostgresConnectionString(IConfiguration configuration)
{
    var configuredConnection = configuration.GetConnectionString("DefaultConnection");
    if (!string.IsNullOrWhiteSpace(configuredConnection))
    {
        return configuredConnection;
    }

    var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
    if (!string.IsNullOrWhiteSpace(databaseUrl) &&
        Uri.TryCreate(databaseUrl, UriKind.Absolute, out var databaseUri))
    {
        var userInfo = databaseUri.UserInfo.Split(':', 2);
        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = databaseUri.Host,
            Port = databaseUri.Port > 0 ? databaseUri.Port : 5432,
            Database = databaseUri.AbsolutePath.TrimStart('/'),
            Username = Uri.UnescapeDataString(userInfo.ElementAtOrDefault(0) ?? string.Empty),
            Password = Uri.UnescapeDataString(userInfo.ElementAtOrDefault(1) ?? string.Empty),
            SslMode = SslMode.Require
        };

        return builder.ConnectionString;
    }

    var host = Environment.GetEnvironmentVariable("PGHOST");
    var database = Environment.GetEnvironmentVariable("PGDATABASE");
    var username = Environment.GetEnvironmentVariable("PGUSER");
    var password = Environment.GetEnvironmentVariable("PGPASSWORD");

    if (string.IsNullOrWhiteSpace(host) ||
        string.IsNullOrWhiteSpace(database) ||
        string.IsNullOrWhiteSpace(username) ||
        string.IsNullOrWhiteSpace(password))
    {
        return null;
    }

    var portValue = Environment.GetEnvironmentVariable("PGPORT");
    var port = int.TryParse(portValue, out var parsedPort) ? parsedPort : 5432;

    return new NpgsqlConnectionStringBuilder
    {
        Host = host,
        Port = port,
        Database = database,
        Username = username,
        Password = password,
        SslMode = SslMode.Require
    }.ConnectionString;
}

static void ValidatePostgresConnectionString(string connectionString)
{
    NpgsqlConnectionStringBuilder builder;
    try
    {
        builder = new NpgsqlConnectionStringBuilder(connectionString);
    }
    catch (Exception ex)
    {
        throw new InvalidOperationException(
            "PostgreSQL connection string is invalid. Use Host=...;Port=...;Database=...;Username=...;Password=... format or set DATABASE_URL.",
            ex);
    }

    if (string.IsNullOrWhiteSpace(builder.Host))
    {
        throw new InvalidOperationException(
            "PostgreSQL connection string is missing Host. Check the Railway ConnectionStrings__DefaultConnection variable.");
    }
}
