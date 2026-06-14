using LifeHub.Application.Interfaces;
using LifeHub.Application.Services;
using LifeHub.Infrastructure.Repository;
using LifeHub.Infrastructure.Security;
using Microsoft.Extensions.DependencyInjection;

namespace LifeHub.Infrastructure
{
    public static class InfrastructureServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            // repositories
            services.AddScoped<IUserRepository, UserRepository>();

            // security primitives
            services.AddSingleton<IPasswordHasher, PasswordHasher>();
            services.AddSingleton<ITokenService, JwtTokenService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IUserService, UserService>();

            // add other registrations
            return services;
        }
    }
}
