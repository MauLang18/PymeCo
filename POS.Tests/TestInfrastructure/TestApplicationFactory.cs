using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

public sealed class TestApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureAppConfiguration(
                (ctx, cfg) =>
                {
                    cfg.AddJsonFile(
                        "appsettings.Testing.json",
                        optional: true,
                        reloadOnChange: true
                    );
                    // No tocar servicios/DbContext aquí
                }
            );
        });
    }
}
