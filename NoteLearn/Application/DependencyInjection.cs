namespace NoteLearn.Application;
using Microsoft.Extensions.DependencyInjection;
public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddMediatR(cfg=>
            cfg.RegisterServicesFromAssembly(typeof(ApplicationAssemblyMarker).Assembly));
        return services;
    }

}
