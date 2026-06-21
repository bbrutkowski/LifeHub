using LifeHub.Application.Interfaces.Services;
using LifeHub.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace LifeHub.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IUserService, UserService>();

            return services;
        }
    }
}
