using LifeHub.Application.Interfaces.Repositories;
using LifeHub.Application.Interfaces.Services;
using LifeHub.Application.Interfaces.Utils;
using LifeHub.Infrastructure.Email;
using LifeHub.Infrastructure.Repository;
using LifeHub.Infrastructure.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LifeHub.Infrastructure
{
    public static class InfrastructureServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IRefreshTokenSessionRepository, RefreshTokenSessionRepository>();
            services.AddSingleton<IPasswordHasher, PasswordHasher>();
            services.AddSingleton<ITokenService, JwtTokenService>();
            services.Configure<EmailSettings>(configuration.GetSection("Email"));
            services.AddScoped<IEmailService, EmailService>();

            return services;
        }
    }
}
