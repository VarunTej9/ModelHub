using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using ModelHub.Api.Contracts;
using ModelHub.Api.Middleware;
using ModelHub.Application.Interfaces;
using ModelHub.Infrastructure.Authentication;
using ModelHub.Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter only the JWT token. Swagger adds Bearer automatically."
    });

    options.AddSecurityRequirement(document =>
        new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", document)] = []
        });
});

builder.Services.AddHealthChecks();

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddScoped<
    IJwtTokenService,
    JwtTokenService>();

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(item => item.Value?.Errors.Count > 0)
            .ToDictionary(
                item => item.Key,
                item => item.Value!.Errors
                    .Select(error =>
                        string.IsNullOrWhiteSpace(error.ErrorMessage)
                            ? "Invalid value."
                            : error.ErrorMessage)
                    .ToArray());

        var response = new ErrorResponse(
            StatusCodes.Status400BadRequest,
            "Validation failed.",
            context.HttpContext.TraceIdentifier,
            errors);

        return new BadRequestObjectResult(response);
    };
});

var jwtSection = builder.Configuration.GetSection("Jwt");

var jwt = jwtSection.Get<JwtOptions>()
    ?? throw new InvalidOperationException(
        "JWT configuration is missing.");

if (string.IsNullOrWhiteSpace(jwt.Key))
{
    throw new InvalidOperationException(
        "Jwt:Key is missing.");
}

builder.Services.Configure<JwtOptions>(jwtSection);

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwt.Key)),

                ValidateIssuer = true,
                ValidIssuer = jwt.Issuer,

                ValidateAudience = true,
                ValidAudience = jwt.Audience,

                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,

                RoleClaimType = ClaimTypes.Role,
                NameClaimType = ClaimTypes.NameIdentifier
            };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(
        "AdminOnly",
        policy => policy.RequireRole("Admin"));

    options.AddPolicy(
        "AnalystOnly",
        policy => policy.RequireRole("Analyst"));

    options.AddPolicy(
        "ReviewerOnly",
        policy => policy.RequireRole("Reviewer"));
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseMiddleware<ExceptionHandlingMiddleware>();
}


app.UseStatusCodePages(async statusCodeContext =>
{
    var context = statusCodeContext.HttpContext;

    if (context.Response.StatusCode is not
        (StatusCodes.Status401Unauthorized
        or StatusCodes.Status403Forbidden))
    {
        return;
    }

    var message = context.Response.StatusCode ==
                  StatusCodes.Status401Unauthorized
        ? "Authentication is required."
        : "You are not authorized to perform this action.";

    var response = new ErrorResponse(
        context.Response.StatusCode,
        message,
        context.TraceIdentifier);

    await context.Response.WriteAsJsonAsync(response);
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks(
    "/health",
    new HealthCheckOptions
    {
        AllowCachingResponses = false
    });

app.Run();

public partial class Program
{
}
