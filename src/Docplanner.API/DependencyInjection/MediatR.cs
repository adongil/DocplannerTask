using Docplanner.Application.Services;
using System.Reflection;

namespace Docplanner.API.DependencyInjection;

public static class MediatR
{
    public static void AddMediatRServices(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.Load("Docplanner.API")));
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.Load("Docplanner.Application")));
    }
}
