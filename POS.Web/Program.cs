using POS.Application;
using POS.Infrastructure;
using Serilog;
using Serilog.Formatting.Compact;

var builder = WebApplication.CreateBuilder(args);

// 1) Configurar Serilog ANTES de construir el host
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration) // lee Serilog de appsettings.*
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithEnvironmentName()
    .CreateLogger();

builder.Host.UseSerilog(); // reemplaza el logger nativo

// 2) Servicios
builder.Services.AddControllersWithViews();

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "AllowAll",
        policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()
    );
});

// Tus capas
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// 3) Middlewares / pipeline
if (app.Environment.IsDevelopment())
{
    // Página de errores con detalles
    app.UseDeveloperExceptionPage();
}
else
{
    // Manejo global de excepciones
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

// Para códigos de estado (404/403/etc.)
app.UseStatusCodePagesWithReExecute("/errors/{0}");

// Log de cada request HTTP (Serilog)
app.UseSerilogRequestLogging(opts =>
{
    // Mensaje por request
    opts.MessageTemplate =
        "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
    // Puedes agregar aquí filtros si quieres excluir health checks, etc.
});

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseCors("AllowAll");

app.UseAuthorization();

// Ruta por defecto (ajústala a tu Home o a Products/Clients)
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

// 4) Arranque + Flush de logs
try
{
    Log.Information("Starting POS.Web");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "POS.Web terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
