using POS.Application.DTOs;
using POS.Infrastructure.Repositories;
using POS.Application.Interfaces;
using POS.Domain.Entities;

namespace POS.Application.Services;

public class ClientService : IClientService
    {
        private readonly IClientRepository _repo;

        public ClientService(IClientRepository repo) => _repo = repo;

        public async Task<int> CreateAsync(ClientDto dto, CancellationToken ct = default)
        {
            var entity = new Client
            {
                Name = dto.Name.Trim(),
                NationalId = dto.NationalId?.Trim(),
                Email = dto.Email?.Trim(),
                Phone = dto.Phone?.Trim(),
                Address = dto.Address?.Trim()
            };

            if (!string.IsNullOrWhiteSpace(entity.NationalId) &&
                await _repo.ExistsByNationalIdAsync(entity.NationalId!, ct))
            {
                throw new InvalidOperationException("Another client already uses this national id.");
            }

            await _repo.AddAsync(entity, ct);
            await _repo.SaveChangesAsync(ct);
            return entity.Id;
        }

        public async Task UpdateAsync(int id, ClientDto dto, CancellationToken ct = default)
        {
            var entity = await _repo.GetByIdAsync(id, ct) ?? throw new KeyNotFoundException("Client not found.");

            // Optional uniqueness check if NationalId changes
            if (!string.Equals(entity.NationalId, dto.NationalId, StringComparison.OrdinalIgnoreCase) &&
                !string.IsNullOrWhiteSpace(dto.NationalId) &&
                await _repo.ExistsByNationalIdAsync(dto.NationalId, ct))
            {
                throw new InvalidOperationException("Another client already uses this national id.");
            }

            entity.Name = dto.Name.Trim();
            entity.NationalId = dto.NationalId?.Trim();
            entity.Email = dto.Email?.Trim();
            entity.Phone = dto.Phone?.Trim();
            entity.Address = dto.Address?.Trim();

            _repo.Update(entity);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            var entity = await _repo.GetByIdAsync(id, ct) ?? throw new KeyNotFoundException("Client not found.");
            _repo.Remove(entity);
            await _repo.SaveChangesAsync(ct);
        }

        public Task<Client?> GetByIdAsync(int id, CancellationToken ct = default)
            => _repo.GetByIdAsync(id, ct);

        public Task<IReadOnlyList<Client>> ListAsync(string? search = null, CancellationToken ct = default)
            => _repo.ListAsync(search, ct);
    }
