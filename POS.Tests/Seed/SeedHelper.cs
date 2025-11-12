using Microsoft.Extensions.DependencyInjection;
using POS.Domain.Entities;
using POS.Domain.Enums;
using POS.Infrastructure.Persistence;

namespace POS.Tests.Seed;

public static class SeedHelper
{
    public static int EnsureOneProduct(IServiceProvider sp)
    {
        using var scope = sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        if (!db.Products.Any())
        {
            var p = new Product
            {
                Name = "Seed Product",
                Price = 999.99m,
                TaxPercent = 13m,
                Stock = 5,
                ImageUrl = "https://example.com/img.png",
                Status = ProductStatus.Active,
                CategoryId = 1, // if FK exists; otherwise remove
            };
            db.Products.Add(p);
            db.SaveChanges();
            return p.Id;
        }
        return db.Products.Select(x => x.Id).First();
    }

    public static int EnsureOneClient(IServiceProvider sp)
    {
        using var scope = sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        if (!db.Clients.Any())
        {
            var c = new Client
            {
                Name = "Seed Client",
                NationalId = "101010101",
                Email = "seed@example.com",
                Phone = "8888-8888",
                Address = "Seed Address",
            };
            db.Clients.Add(c);
            db.SaveChanges();
            return c.Id;
        }
        return db.Clients.Select(x => x.Id).First();
    }
}
