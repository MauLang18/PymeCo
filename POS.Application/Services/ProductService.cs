using POS.Application.DTOs;
using POS.Application.Interfaces;
using POS.Domain.Entities;
using POS.Domain.Enums;
using POS.Infrastructure.Repositories;

namespace POS.Application.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _repo;

    public ProductService(IProductRepository repo) => _repo = repo;

    public async Task<int> CreateAsync(ProductDto dto, CancellationToken ct = default)
    {
        if (await _repo.ExistsByNameAsync(dto.Name.Trim(), ct))
            throw new InvalidOperationException("Product name already exists.");

        var entity = new Product
        {
            Name = dto.Name.Trim(),
            CategoryId = dto.CategoryId,
            Price = dto.Price,
            TaxPercent = dto.TaxPercent,
            Stock = dto.Stock,
            ImageUrl = dto.ImageUrl,
            Status = dto.Active ? ProductStatus.Active : ProductStatus.Inactive,
        };

        await _repo.AddAsync(entity, ct);
        await _repo.SaveChangesAsync(ct);
        return entity.Id;
    }

    public async Task UpdateAsync(int id, ProductDto dto, CancellationToken ct = default)
    {
        var entity =
            await _repo.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("Product not found.");
        entity.Name = dto.Name.Trim();
        entity.CategoryId = dto.CategoryId;
        entity.Price = dto.Price;
        entity.TaxPercent = dto.TaxPercent;
        entity.Stock = dto.Stock;
        entity.ImageUrl = dto.ImageUrl;
        entity.Status = dto.Active ? ProductStatus.Active : ProductStatus.Inactive;

        _repo.Update(entity);
        await _repo.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity =
            await _repo.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("Product not found.");
        _repo.Remove(entity);
        await _repo.SaveChangesAsync(ct);
    }

    public Task<Product?> GetByIdAsync(int id, CancellationToken ct = default) =>
        _repo.GetByIdAsync(id, ct);

    public Task<IReadOnlyList<Product>> ListAsync(
        string? search = null,
        CancellationToken ct = default
    ) => _repo.ListAsync(search, ct);
}
