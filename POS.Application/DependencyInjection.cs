using Microsoft.Extensions.DependencyInjection;
using POS.Application.Interfaces;
using POS.Application.Services;

namespace POS.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Services (casos de uso)TargetFramework
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IClientService, ClientService>();
        services.AddScoped<IProductService, ProductService>();

        // Si usas AutoMapper/FluentValidation/MediatR, agrégalos aquí.
        // services.AddAutoMapper(typeof(DependencyInjection).Assembly);
        // services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
        return services;
    }
}
