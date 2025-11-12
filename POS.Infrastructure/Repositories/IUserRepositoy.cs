using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using POS.Domain.Entities;

namespace POS.Infrastructure.Repositories
{
    public interface IUserRepository
    {
        Task<int> CreateAsync(Usuario user, CancellationToken ct = default);
        Task<Usuario?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<IReadOnlyList<Usuario>> ListAsync(CancellationToken ct = default);
        Task<bool> UpdateAsync(Usuario user, CancellationToken ct = default);
        Task<bool> DeleteAsync(Usuario user, CancellationToken ct = default);
    }
}
