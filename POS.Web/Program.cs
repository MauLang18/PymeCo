using System.IO;
using Microsoft.AspNetCore.Identity;
using POS.Application;
using POS.Domain.Entities;
using POS.Infrastructure;
using POS.Infrastructure.Persistence;
using Serilog;

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
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

// Register layers
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration); // ✅ Solo una vez

// ========== CONFIGURACIÓN DE IDENTITY ==========
builder
    .Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        // Configuración de contraseña
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 6;

        // Configuración de usuario
        options.User.RequireUniqueEmail = true;

        // Configuración de lockout (bloqueo por intentos fallidos)
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.AllowedForNewUsers = true;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// Configuración de cookies de autenticación
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Auth/Login";
    options.LogoutPath = "/Auth/Logout";
    options.AccessDeniedPath = "/Auth/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
});

// ===============================================

var app = builder.Build();

// ========== SEED DE ROLES Y USUARIOS ==========
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    await SeedRolesAndUsers(roleManager, userManager);
}

// ==================================================

// 3) Middlewares / pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
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

// ⚠️ IMPORTANTE: El orden es crítico
app.UseAuthentication(); // Primero autenticación
app.UseAuthorization(); // Luego autorización

// Crear directorio de uploads si no existe
var uploadsPath = Path.Combine(app.Environment.WebRootPath!, "uploads", "products");
if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
    Log.Information("Directorio de uploads creado: {Path}", uploadsPath);
}

app.MapControllerRoute(name: "default", pattern: "{controller=Auth}/{action=Login}/{id?}");

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

// ========== MÉTODO PARA CREAR ROLES Y USUARIOS ==========
static async Task SeedRolesAndUsers(
    RoleManager<IdentityRole> roleManager,
    UserManager<ApplicationUser> userManager
)
{
    // Crear roles si no existen
    string[] roles = { "Admin", "Cajero", "Vendedor" };

    foreach (var roleName in roles)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
            Log.Information("Rol creado: {RoleName}", roleName);
        }
    }

    // ========== CREAR USUARIO ADMIN ==========
    var adminEmail = "admin@pos.com";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);

    if (adminUser == null)
    {
        adminUser = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            FullName = "Administrador",
            EmailConfirmed = true,
            IsActive = true,
        };

        var result = await userManager.CreateAsync(adminUser, "Admin123!");

        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
            Log.Information("Usuario Admin creado: {Email}", adminEmail);
        }
        else
        {
            Log.Error(
                "Error al crear Admin: {Errors}",
                string.Join(", ", result.Errors.Select(e => e.Description))
            );
        }
    }

    // ========== CREAR USUARIO VENDEDOR ==========
    var vendedorEmail = "vendedor@pos.com";
    var vendedorUser = await userManager.FindByEmailAsync(vendedorEmail);

    if (vendedorUser == null)
    {
        vendedorUser = new ApplicationUser
        {
            UserName = vendedorEmail,
            Email = vendedorEmail,
            FullName = "María Vendedor",
            EmailConfirmed = true,
            IsActive = true,
        };

        var result = await userManager.CreateAsync(vendedorUser, "Vendedor123!");

        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(vendedorUser, "Vendedor");
            Log.Information("Usuario Vendedor creado: {Email}", vendedorEmail);
        }
        else
        {
            Log.Error(
                "Error al crear Vendedor: {Errors}",
                string.Join(", ", result.Errors.Select(e => e.Description))
            );
        }
    }

    // ========== CREAR USUARIO CAJERO ==========
    var cajeroEmail = "cajero@pos.com";
    var cajeroUser = await userManager.FindByEmailAsync(cajeroEmail);

    if (cajeroUser == null)
    {
        cajeroUser = new ApplicationUser
        {
            UserName = cajeroEmail,
            Email = cajeroEmail,
            FullName = "Juan Cajero",
            EmailConfirmed = true,
            IsActive = true,
        };

        var result = await userManager.CreateAsync(cajeroUser, "Cajero123!");

        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(cajeroUser, "Cajero");
            Log.Information("Usuario Cajero creado: {Email}", cajeroEmail);
        }
        else
        {
            Log.Error(
                "Error al crear Cajero: {Errors}",
                string.Join(", ", result.Errors.Select(e => e.Description))
            );
        }
    }
}

// ⭐ Hacer Program accesible para tests de integración
public partial class Program { }
