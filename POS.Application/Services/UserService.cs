using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using POS.Application.Interfaces;
using POS.Domain.Entities;
using POS.Infrastructure.Repositories;

namespace POS.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repo;
        private readonly ILogger<UserService> _logger;

        public UserService(IUserRepository repo, ILogger<UserService> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        public async Task<int> CreateAsync(Usuario user, CancellationToken ct = default)
        {
            _logger.LogInformation("Creating Usuario mirror for {Nombre}", user.Nombre);
            return await _repo.CreateAsync(user, ct);
        }

        public async Task<Usuario?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _repo.GetByIdAsync(id, ct);
        }

        public async Task<IReadOnlyList<Usuario>> ListAsync(CancellationToken ct = default)
        {
            return await _repo.ListAsync(ct);
        }

        public async Task<bool> UpdateAsync(int id, Usuario user, CancellationToken ct = default)
        {
            var existing = await _repo.GetByIdAsync(id, ct);
            if (existing is null)
                return false;

            existing.Nombre = user.Nombre;
            existing.EstadoUsuario = user.EstadoUsuario;
            existing.RolId = user.RolId;

            return await _repo.UpdateAsync(existing, ct);
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
        {
            var existing = await _repo.GetByIdAsync(id, ct);
            if (existing is null)
                return false;
            return await _repo.DeleteAsync(existing, ct);
        }
    }
}
