using Docplanner.Application.Services;
using Docplanner.Infrastructure.Client;

namespace Docplanner.API.DependencyInjection
{
    public static class ApplicationServices
    {
        public static void AddApplicationServices(this IServiceCollection services)
        {
            services.AddTransient<IAvailavilityService, AvailavilityService>();
            services.AddTransient<IAvailabilityServiceClient, AvailabilityServiceClient>();
            services.AddTransient<IHttpContextAccessor, HttpContextAccessor>();
        }
    }
}
