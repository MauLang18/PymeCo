using POS.Domain.Entities;

namespace POS.Infrastructure.Repositories;

public interface IClientRepository
{
    Task AddAsync(Client entity, CancellationToken ct = default);
    void Update(Client entity);
    void Remove(Client entity);

    Task<Client?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<Client>> ListAsync(string? search = null, CancellationToken ct = default);

    Task<bool> ExistsByNationalIdAsync(string nationalId, CancellationToken ct = default);

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
