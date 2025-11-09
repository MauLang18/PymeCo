using Microsoft.EntityFrameworkCore;
using POS.Domain.Entities;
using POS.Domain.Enums;
using POS.Infrastructure.Persistence;

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
        string? search = null,
        CancellationToken ct = default
    )
    {
        var q = _db.Products.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
            q = q.Where(p => p.Name.Contains(search));
        return await q.OrderBy(p => p.Name).ToListAsync(ct);
    }

    public Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default) =>
        _db.Products.AnyAsync(p => p.Name == name, ct);

    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
