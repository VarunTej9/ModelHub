using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModelHub.Application.Interfaces;
using ModelHub.Infrastructure.Persistence;
using ModelHub.Application.Services;
using ModelHub.Infrastructure.Authentication;

namespace ModelHub.Infrastructure.DependencyInjection;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("ModelHub") ?? throw new InvalidOperationException("Connection string 'ModelHub' was not found.");

        services.AddDbContext<ModelHubDbContext>(options => options.UseSqlite(connectionString));

        services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));

        services.AddScoped<IUnitOfWork, EfUnitOfWork>();

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

        services.AddScoped<IUserRepository, EfUserRepository>();

        services.AddScoped<IPasswordService, PasswordService>();

        services.AddScoped<IJwtTokenService, JwtTokenService>();

        services.AddScoped<IAuthService, AuthService>();


        return services;
    }
}
