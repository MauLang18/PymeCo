using POS.Application.DTOs;
using POS.Domain.Entities;

namespace POS.Application.Interfaces;

public interface IProductService
{
    Task<int> CreateAsync(ProductDto dto, CancellationToken ct = default);
    Task UpdateAsync(int id, ProductDto dto, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);

    Task<Product?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<Product>> ListAsync(string? search = null, CancellationToken ct = default);
}
