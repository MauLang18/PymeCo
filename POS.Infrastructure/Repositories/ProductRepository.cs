using Microsoft.EntityFrameworkCore;
using POS.Domain.Entities;
using POS.Infrastructure.Persistence;
using POS.Domain.Enums;

namespace POS.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _db;

    public ProductRepository(AppDbContext db) => _db = db;

    public Task AddAsync(Product entity, CancellationToken ct = default) =>
        _db.Products.AddAsync(entity, ct).AsTask();

    public void Update(Product entity) => _db.Products.Update(entity);

    public void Remove(Product entity) => _db.Products.Remove(entity);

    public Task<Product?> GetByIdAsync(int id, CancellationToken ct = default) =>
        _db.Products.FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<IReadOnlyList<Product>> ListAsync(
    string? search = null, CancellationToken ct = default)
    {
        var q = _db.Products.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            // Para buscador (dropdown): solo activos con stock y máx. 10
            q = q.Where(p => p.Status == ProductStatus.Active
                          && p.Stock > 0
                          && p.Name.Contains(term))
                 .OrderBy(p => p.Name)
                 .Take(10);
        }
        else
        {
            // Para la página de listado: muestra todos, ordenados
            q = q.OrderBy(p => p.Name);
            // q = q.Where(p => p.Status == ProductStatus.Active).OrderBy(p => p.Name);
        }

        return await q.ToListAsync(ct);
    }

    public Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default) =>
        _db.Products.AnyAsync(p => p.Name == name, ct);

    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
