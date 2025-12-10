using System.IO;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using POS.Application;
using POS.Domain.Entities;
using POS.Domain.Enums;
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
builder.Services.AddInfrastructure(builder.Configuration);

// ========== CONFIGURACIÓN DE IDENTITY ==========
builder
    .Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 6;
        options.User.RequireUniqueEmail = true;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.AllowedForNewUsers = true;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

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

// ========== APLICAR MIGRACIONES Y SEED DE DATOS ==========
using (var scope = app.Services.CreateScope())
{
    try
    {
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // IMPORTANTE: Aplicar migraciones primero
        Log.Information("📦 Aplicando migraciones...");
        await context.Database.MigrateAsync();
        Log.Information("✅ Migraciones aplicadas correctamente");

        // Ahora hacer el seeding
        await SeedRolesAndUsers(roleManager, userManager);
        await SeedProducts(context);
        await SeedClients(context);
        await SeedPedidos(context, userManager);
    }
    catch (Exception ex)
    {
        Log.Error(ex, "❌ Error durante migraciones o seeding");
        // No lanzar la excepción, permitir que la app inicie de todas formas
    }
}
// ==================================================

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseStatusCodePagesWithReExecute("/errors/{0}");

app.UseSerilogRequestLogging(opts =>
{
    opts.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
});

app.UseStaticFiles();
app.UseRouting();
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

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

// ========== SEED DE ROLES Y USUARIOS ==========
static async Task SeedRolesAndUsers(
    RoleManager<IdentityRole> roleManager,
    UserManager<ApplicationUser> userManager
)
{
    string[] roles = { "Admin", "Cajero", "Vendedor" };

    foreach (var roleName in roles)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
            Log.Information("✅ Rol creado: {RoleName}", roleName);
        }
    }

    // Admin
    var adminEmail = "admin@pos.com";
    if (await userManager.FindByEmailAsync(adminEmail) == null)
    {
        var adminUser = new ApplicationUser
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
            Log.Information("✅ Usuario Admin creado: {Email}", adminEmail);
        }
    }

    // Vendedor
    var vendedorEmail = "vendedor@pos.com";
    if (await userManager.FindByEmailAsync(vendedorEmail) == null)
    {
        var vendedorUser = new ApplicationUser
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
            Log.Information("✅ Usuario Vendedor creado: {Email}", vendedorEmail);
        }
    }

    // Cajero
    var cajeroEmail = "cajero@pos.com";
    if (await userManager.FindByEmailAsync(cajeroEmail) == null)
    {
        var cajeroUser = new ApplicationUser
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
            Log.Information("✅ Usuario Cajero creado: {Email}", cajeroEmail);
        }
    }
}

// ========== SEED DE PRODUCTOS ==========
static async Task SeedProducts(AppDbContext context)
{
    if (context.Products.Any())
    {
        Log.Information("Productos ya existen, saltando seed");
        return;
    }

    var products = new List<Product>
    {
        new Product
        {
            Name = "Laptop Dell XPS 15",
            CategoryId = 1,
            Price = 1200.00m,
            TaxPercent = 13.00m,
            Stock = 10,
            Status = ProductStatus.Active,
            ImageUrl = null,
            CreatedAt = DateTime.Now
        },
        new Product
        {
            Name = "Mouse Logitech MX Master 3",
            CategoryId = 1,
            Price = 99.99m,
            TaxPercent = 13.00m,
            Stock = 25,
            Status = ProductStatus.Active,
            ImageUrl = null,
            CreatedAt = DateTime.Now
        },
        new Product
        {
            Name = "Teclado Mecánico Keychron K2",
            CategoryId = 1,
            Price = 89.99m,
            TaxPercent = 13.00m,
            Stock = 15,
            Status = ProductStatus.Active,
            ImageUrl = null,
            CreatedAt = DateTime.Now
        },
        new Product
        {
            Name = "Monitor LG UltraWide 34\"",
            CategoryId = 1,
            Price = 499.99m,
            TaxPercent = 13.00m,
            Stock = 8,
            Status = ProductStatus.Active,
            ImageUrl = null,
            CreatedAt = DateTime.Now
        },
        new Product
        {
            Name = "Silla Ergonómica Herman Miller",
            CategoryId = 2,
            Price = 899.00m,
            TaxPercent = 13.00m,
            Stock = 5,
            Status = ProductStatus.Active,
            ImageUrl = null,
            CreatedAt = DateTime.Now
        },
        new Product
        {
            Name = "Escritorio Standing Desk",
            CategoryId = 2,
            Price = 549.99m,
            TaxPercent = 13.00m,
            Stock = 12,
            Status = ProductStatus.Active,
            ImageUrl = null,
            CreatedAt = DateTime.Now
        },
        new Product
        {
            Name = "Webcam Logitech C920",
            CategoryId = 1,
            Price = 79.99m,
            TaxPercent = 13.00m,
            Stock = 20,
            Status = ProductStatus.Active,
            ImageUrl = null,
            CreatedAt = DateTime.Now
        },
        new Product
        {
            Name = "Micrófono Blue Yeti",
            CategoryId = 1,
            Price = 129.99m,
            TaxPercent = 13.00m,
            Stock = 18,
            Status = ProductStatus.Active,
            ImageUrl = null,
            CreatedAt = DateTime.Now
        },
        new Product
        {
            Name = "Audífonos Sony WH-1000XM5",
            CategoryId = 1,
            Price = 399.99m,
            TaxPercent = 13.00m,
            Stock = 22,
            Status = ProductStatus.Active,
            ImageUrl = null,
            CreatedAt = DateTime.Now
        },
        new Product
        {
            Name = "Tablet iPad Air",
            CategoryId = 1,
            Price = 599.00m,
            TaxPercent = 13.00m,
            Stock = 14,
            Status = ProductStatus.Active,
            ImageUrl = null,
            CreatedAt = DateTime.Now
        }
    };

    context.Products.AddRange(products);
    await context.SaveChangesAsync();
    Log.Information("✅ Seeded {Count} productos", products.Count);
}

// ========== SEED DE CLIENTES ==========
static async Task SeedClients(AppDbContext context)
{
    if (context.Clients.Any())
    {
        Log.Information("Clientes ya existen, saltando seed");
        return;
    }

    var clients = new List<Client>
    {
        new Client
        {
            Name = "Carlos Rodríguez",
            NationalId = "1-0234-0567",
            Email = "carlos.rodriguez@email.com",
            Phone = "8888-1234",
            Address = "San José, Costa Rica",
            CreatedAt = DateTime.Now
        },
        new Client
        {
            Name = "Ana María González",
            NationalId = "2-0345-0678",
            Email = "ana.gonzalez@email.com",
            Phone = "8888-2345",
            Address = "Heredia, Costa Rica",
            CreatedAt = DateTime.Now
        },
        new Client
        {
            Name = "Roberto Jiménez",
            NationalId = "1-0456-0789",
            Email = "roberto.jimenez@email.com",
            Phone = "8888-3456",
            Address = "Alajuela, Costa Rica",
            CreatedAt = DateTime.Now
        },
        new Client
        {
            Name = "Laura Pérez",
            NationalId = "3-0567-0890",
            Email = "laura.perez@email.com",
            Phone = "8888-4567",
            Address = "Cartago, Costa Rica",
            CreatedAt = DateTime.Now
        },
        new Client
        {
            Name = "Miguel Ángel Vargas",
            NationalId = "1-0678-0901",
            Email = "miguel.vargas@email.com",
            Phone = "8888-5678",
            Address = "San José, Escazú",
            CreatedAt = DateTime.Now
        },
        new Client
        {
            Name = "Patricia Mora",
            NationalId = "2-0789-0123",
            Email = "patricia.mora@email.com",
            Phone = "8888-6789",
            Address = "San José, Santa Ana",
            CreatedAt = DateTime.Now
        },
        new Client
        {
            Name = "José Luis Ramírez",
            NationalId = "1-0890-0234",
            Email = "jose.ramirez@email.com",
            Phone = "8888-7890",
            Address = "Heredia, Santo Domingo",
            CreatedAt = DateTime.Now
        },
        new Client
        {
            Name = "Carmen Solís",
            NationalId = "3-0901-0345",
            Email = "carmen.solis@email.com",
            Phone = "8888-8901",
            Address = "San José, Curridabat",
            CreatedAt = DateTime.Now
        }
    };

    context.Clients.AddRange(clients);
    await context.SaveChangesAsync();
    Log.Information("✅ Seeded {Count} clientes", clients.Count);
}

// ========== SEED DE PEDIDOS ==========
static async Task SeedPedidos(AppDbContext context, UserManager<ApplicationUser> userManager)
{
    try
    {
        if (context.Set<Pedido>().Any())
        {
            Log.Information("Pedidos ya existen, saltando seed");
            return;
        }

        var clientes = await context.Clients.Take(4).ToListAsync();
        var productos = await context.Products.Take(7).ToListAsync();

        if (!clientes.Any() || !productos.Any())
        {
            Log.Warning("⚠️ No hay clientes o productos, saltando seed de pedidos");
            return;
        }

        // Obtener un usuario de Identity
        var adminUser = await userManager.FindByEmailAsync("admin@pos.com");
        if (adminUser == null)
        {
            Log.Warning("⚠️ No hay usuario admin, saltando seed de pedidos");
            return;
        }

        string usuarioId = adminUser.Id;

        var pedidos = new List<Pedido>
        {
            // Pedido 1: Pendiente
            new Pedido
            {
                ClienteId = clientes[0].Id,
                UsuarioId = usuarioId,
                Fecha = DateTime.Now.AddDays(-5),
                Estado = "Pendiente",
                Subtotal = 1389.98m,
                Impuestos = 180.70m,
                Total = 1570.68m,
                Detalles = new List<PedidoDetalle>
                {
                    new PedidoDetalle
                    {
                        ProductoId = productos[0].Id,
                        Cantidad = 1,
                        PrecioUnit = 1200.00m,
                        Descuento = 0,
                        ImpuestoPorc = 13.00m,
                        TotalLinea = 1356.00m
                    },
                    new PedidoDetalle
                    {
                        ProductoId = productos[1].Id,
                        Cantidad = 2,
                        PrecioUnit = 99.99m,
                        Descuento = 10,
                        ImpuestoPorc = 13.00m,
                        TotalLinea = 203.58m
                    }
                }
            },

            // Pedido 2: Pagado
            new Pedido
            {
                ClienteId = clientes[1].Id,
                UsuarioId = usuarioId,
                Fecha = DateTime.Now.AddDays(-3),
                Estado = "Pagado",
                Subtotal = 589.98m,
                Impuestos = 76.70m,
                Total = 666.68m,
                Detalles = new List<PedidoDetalle>
                {
                    new PedidoDetalle
                    {
                        ProductoId = productos[3].Id,
                        Cantidad = 1,
                        PrecioUnit = 499.99m,
                        Descuento = 0,
                        ImpuestoPorc = 13.00m,
                        TotalLinea = 564.99m
                    },
                    new PedidoDetalle
                    {
                        ProductoId = productos[2].Id,
                        Cantidad = 1,
                        PrecioUnit = 89.99m,
                        Descuento = 0,
                        ImpuestoPorc = 13.00m,
                        TotalLinea = 101.69m
                    }
                }
            },

            // Pedido 3: Enviado
            new Pedido
            {
                ClienteId = clientes[2].Id,
                UsuarioId = usuarioId,
                Fecha = DateTime.Now.AddDays(-1),
                Estado = "Enviado",
                Subtotal = 899.00m,
                Impuestos = 116.87m,
                Total = 1015.87m,
                Detalles = new List<PedidoDetalle>
                {
                    new PedidoDetalle
                    {
                        ProductoId = productos[4].Id,
                        Cantidad = 1,
                        PrecioUnit = 899.00m,
                        Descuento = 0,
                        ImpuestoPorc = 13.00m,
                        TotalLinea = 1015.87m
                    }
                }
            },

            // Pedido 4: Pendiente reciente
            new Pedido
            {
                ClienteId = clientes[3].Id,
                UsuarioId = usuarioId,
                Fecha = DateTime.Now,
                Estado = "Pendiente",
                Subtotal = 179.98m,
                Impuestos = 23.40m,
                Total = 203.38m,
                Detalles = new List<PedidoDetalle>
                {
                    new PedidoDetalle
                    {
                        ProductoId = productos[1].Id,
                        Cantidad = 1,
                        PrecioUnit = 99.99m,
                        Descuento = 0,
                        ImpuestoPorc = 13.00m,
                        TotalLinea = 112.99m
                    },
                    new PedidoDetalle
                    {
                        ProductoId = productos[6].Id,
                        Cantidad = 1,
                        PrecioUnit = 79.99m,
                        Descuento = 0,
                        ImpuestoPorc = 13.00m,
                        TotalLinea = 90.39m
                    }
                }
            }
        };

        context.Set<Pedido>().AddRange(pedidos);
        await context.SaveChangesAsync();
        Log.Information("✅ Seeded {Count} pedidos con sus detalles", pedidos.Count);
    }
    catch (Exception ex)
    {
        Log.Error(ex, "❌ Error al crear pedidos de prueba");
    }
}

public partial class Program { }