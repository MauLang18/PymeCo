using POS.Application;
using POS.Infrastructure;
using Serilog;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// Serilog first
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithEnvironmentName()
    .CreateLogger();

builder.Host.UseSerilog();

// Configuration (includes Testing)
builder
    .Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile(
        $"appsettings.{builder.Environment.EnvironmentName}.json",
        optional: true,
        reloadOnChange: true
    )
    .AddJsonFile("appsettings.Testing.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// Services
builder.Services.AddControllersWithViews();
builder.Services.AddInfrastructure(builder.Configuration); // ⬅️ (una sola vez)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

// Register layers
builder.Services.AddApplication();
// builder.Services.AddInfrastructure(builder.Configuration); // ⬅️ eliminado el duplicado

var app = builder.Build();

// Testing flag (used to relax pipeline under test host)
var isTesting = app.Environment.IsEnvironment("Testing");

// Middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else if (!isTesting)
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

if (!isTesting)
{
    app.UseHttpsRedirection();
}

app.UseStatusCodePagesWithReExecute("/errors/{0}");
app.UseSerilogRequestLogging(opts =>
{
    opts.MessageTemplate =
        "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
});

app.UseStaticFiles();
app.UseRouting();
app.UseCors("AllowAll");
app.UseAuthorization();

// Ensure the uploads folder exists
Directory.CreateDirectory(Path.Combine(app.Environment.WebRootPath!, "uploads", "products"));

app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

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

// Required for WebApplicationFactory<T>
public partial class Program { }
