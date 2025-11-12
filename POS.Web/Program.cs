using Microsoft.AspNetCore.Identity;
using POS.Application;
using POS.Domain.Entities;
using POS.Infrastructure;
using POS.Infrastructure.Persistence;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// 1) Configurar Serilog ANTES de construir el host
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithEnvironmentName()
    .CreateLogger();

builder.Host.UseSerilog();

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

// ========== CONFIGURACIÓN DE IDENTITY ==========
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
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

// ========== SEED DE ROLES Y USUARIO ADMIN ==========
// Esto se ejecuta una vez al inicio para crear roles
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    await SeedRolesAndAdminUser(roleManager, userManager);
}
// ==================================================

// 3) Middlewares / pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/errors/{0}");

app.UseSerilogRequestLogging(opts =>
{
    opts.MessageTemplate =
        "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
});

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseCors("AllowAll");

// ⚠️ IMPORTANTE: El orden es crítico
app.UseAuthentication(); // Primero autenticación
app.UseAuthorization();  // Luego autorización

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

// ========== MÉTODO PARA CREAR ROLES Y USUARIO ADMIN ==========
static async Task SeedRolesAndAdminUser(
    RoleManager<IdentityRole> roleManager,
    UserManager<ApplicationUser> userManager)
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

    // Crear usuario Admin si no existe
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
            IsActive = true
        };

        var result = await userManager.CreateAsync(adminUser, "Admin123!");

        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
            Log.Information("Usuario Admin creado: {Email}", adminEmail);
        }
    }
}