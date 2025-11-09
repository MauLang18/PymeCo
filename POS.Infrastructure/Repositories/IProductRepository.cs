using POS.Domain.Entities;

namespace POS.Infrastructure.Repositories;

public interface IProductRepository {
        Task AddAsync(Product entity, CancellationToken ct = default);
        void Update(Product entity);
        void Remove(Product entity);

        Task<Product?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<IReadOnlyList<Product>> ListAsync(string? search = null, CancellationToken ct = default);

        Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default);

        Task<int> SaveChangesAsync(CancellationToken ct = default);
    }
