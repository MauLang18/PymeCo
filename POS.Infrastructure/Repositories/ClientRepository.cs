using Microsoft.EntityFrameworkCore;
using POS.Domain.Entities;
using POS.Infrastructure.Persistence;

namespace POS.Infrastructure.Repositories;

public class ClientRepository : IClientRepository
{
    private readonly AppDbContext _db;

    public ClientRepository(AppDbContext db) => _db = db;

    public Task AddAsync(Client entity, CancellationToken ct = default) =>
        _db.Clients.AddAsync(entity, ct).AsTask();

    public void Update(Client entity) => _db.Clients.Update(entity);

    public void Remove(Client entity) => _db.Clients.Remove(entity);

    public Task<Client?> GetByIdAsync(int id, CancellationToken ct = default) =>
        _db.Clients.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<IReadOnlyList<Client>> ListAsync(
        string? search = null,
        CancellationToken ct = default
    )
    {
        var q = _db.Clients.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
        {
            q = q.Where(x =>
                x.Name!.Contains(search)
                || (x.NationalId != null && x.NationalId.Contains(search))
                || (x.Email != null && x.Email.Contains(search))
            );
        }
        return await q.OrderBy(x => x.Name).ToListAsync(ct);
    }

    public Task<bool> ExistsByNationalIdAsync(string nationalId, CancellationToken ct = default) =>
        _db.Clients.AnyAsync(x => x.NationalId == nationalId, ct);

    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
