using POS.Application.DTOs;
using POS.Domain.Entities;

namespace POS.Application.Interfaces;

public interface IClientService
    {
        Task<int> CreateAsync(ClientDto dto, CancellationToken ct = default);
        Task UpdateAsync(int id, ClientDto dto, CancellationToken ct = default);
        Task DeleteAsync(int id, CancellationToken ct = default);

        Task<Client?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<IReadOnlyList<Client>> ListAsync(string? search = null, CancellationToken ct = default);
    }
