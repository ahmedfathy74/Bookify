using Microsoft.Extensions.DependencyInjection;

namespace Bookify.Application
{
    public static class ConfigureServices
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IAuthorService, AuthorService>();
            services.AddScoped<IAreaService, AreaService>();

            return services;
        }
    }
}
