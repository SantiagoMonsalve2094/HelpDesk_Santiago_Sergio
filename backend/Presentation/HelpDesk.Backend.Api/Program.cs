using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using HelpDesk.Backend.Api.Errors;
using HelpDesk.Backend.Api.Middleware;
using HelpDesk.Backend.Api.ModelBinding;
using HelpDesk.Backend.Application;
using HelpDesk.Backend.Infrastructure;
using HelpDesk.Backend.Infrastructure.Persistence;
using HelpDesk.Backend.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services
    .AddControllers(options =>
    {
        options.ModelBinderProviders.Insert(0, new StringEnumModelBinderProvider());
    })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, false));
    });
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(entry => entry.Value?.Errors.Count > 0)
            .SelectMany(entry => entry.Value!.Errors.Select(error =>
                new ApiErrorDetail(
                    "INVALID_REQUEST",
                    string.IsNullOrWhiteSpace(error.ErrorMessage)
                        ? "El valor enviado no es válido."
                        : error.ErrorMessage,
                    ToCamelCase(entry.Key))))
            .ToArray();
        return new BadRequestObjectResult(
            new ApiErrorResponse(
                StatusCodes.Status400BadRequest,
                "Solicitud inválida",
                "Uno o más campos de la solicitud son inválidos.",
                context.HttpContext.TraceIdentifier,
                errors));
    };
});

var jwtOptions = builder.Configuration
    .GetSection(JwtOptions.SectionName)
    .Get<JwtOptions>() ?? new JwtOptions();
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            NameClaimType = JwtRegisteredClaimNames.Name,
            RoleClaimType = "role"
        };
        options.Events = new JwtBearerEvents
        {
            OnChallenge = async context =>
            {
                context.HandleResponse();
                await ApiErrorWriter.WriteAsync(
                    context.HttpContext,
                    StatusCodes.Status401Unauthorized,
                    "No autenticado",
                    "Se requiere un token de acceso válido.");
            },
            OnForbidden = context => ApiErrorWriter.WriteAsync(
                context.HttpContext,
                StatusCodes.Status403Forbidden,
                "Acceso denegado",
                "El usuario no tiene permisos para ejecutar esta operación.")
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        var origins = builder.Environment.IsDevelopment()
            ? ["http://localhost:5173"]
            : builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

        if (origins.Length > 0)
        {
            policy.WithOrigins(origins)
                .AllowAnyHeader()
                .AllowAnyMethod();
        }
        else
        {
            policy.SetIsOriginAllowed(_ => false);
        }
    });
});
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy(
        "login",
        httpContext => RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            }));
    options.OnRejected = async (context, _) =>
    {
        await ApiErrorWriter.WriteAsync(
            context.HttpContext,
            StatusCodes.Status429TooManyRequests,
            "Demasiados intentos",
            "Se permiten máximo cinco intentos de inicio de sesión por minuto.");
    };
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc(
        "v1",
        new OpenApiInfo
        {
            Title = "HelpDesk Backend API",
            Version = "v1"
        });
    var bearerScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Ingrese el token JWT con el prefijo Bearer.",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = "Bearer"
        }
    };
    options.AddSecurityDefinition("Bearer", bearerScheme);
    options.AddSecurityRequirement(
        new OpenApiSecurityRequirement
        {
            [bearerScheme] = []
        });
});

builder.Services.AddHealthChecks()
    .AddDbContextCheck<HelpDeskDbContext>("sql-server", tags: ["ready"]);

var app = builder.Build();

app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("Frontend");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks(
        "/health/live",
        new HealthCheckOptions { Predicate = _ => false })
    .AllowAnonymous();
app.MapHealthChecks(
        "/health/ready",
        new HealthCheckOptions
        {
            Predicate = registration => registration.Tags.Contains("ready")
        })
    .AllowAnonymous();

await using (var scope = app.Services.CreateAsyncScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
    await initializer.InitializeAsync(
        app.Environment.IsDevelopment(),
        app.Lifetime.ApplicationStopping);
}

await app.RunAsync();

static string? ToCamelCase(string? value)
{
    if (string.IsNullOrWhiteSpace(value))
    {
        return value;
    }

    var separatorIndex = value.LastIndexOf('.');
    var field = separatorIndex >= 0 ? value[(separatorIndex + 1)..] : value;
    return char.ToLowerInvariant(field[0]) + field[1..];
}

public partial class Program;
