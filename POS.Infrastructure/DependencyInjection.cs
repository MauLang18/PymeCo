using DinkToPdf;
using DinkToPdf.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using POS.Infrastructure.FileStorage;
using POS.Infrastructure.GenerateExcel;
using POS.Infrastructure.GeneratePdf;
using POS.Infrastructure.Persistence;
using POS.Infrastructure.Repositories;

namespace POS.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration config
    )
    {
        var provider = (config["Database:Provider"] ?? "SqlServer").Trim();

        if (provider.Equals("InMemory", StringComparison.OrdinalIgnoreCase))
        {
            var dbName = config["Database:InMemoryName"] ?? "POS_TestDb";
            services.AddDbContext<AppDbContext>(opt =>
            {
                opt.UseInMemoryDatabase(dbName);
            });
        }
        else
        {
            var conn =
                config.GetConnectionString("Default")
                ?? throw new InvalidOperationException("Missing ConnectionStrings:Default");
            services.AddDbContext<AppDbContext>(opt =>
            {
                opt.UseSqlServer(conn);
            });
        }

        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IClientRepository, ClientRepository>();
        services.AddScoped<IPedidoRepository, PedidoRepository>();
        services.AddSingleton<IFileStorageLocal, FileStorageLocal>();
        services.AddTransient<IGenerateExcelService, GenerateExcelService>();
        services.AddTransient<IGeneratePdfService, GeneratePdfService>();

        services.AddSingleton<IConverter, SynchronizedConverter>(
            provider => new SynchronizedConverter(new PdfTools())
        );

        return services;
    }
}
