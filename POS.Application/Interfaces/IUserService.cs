using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using POS.Domain.Entities;

namespace POS.Application.Interfaces
{
    public interface IUserService
    {
        Task<int> CreateAsync(Usuario user, CancellationToken ct = default);
        Task<Usuario?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<IReadOnlyList<Usuario>> ListAsync(CancellationToken ct = default);
        Task<bool> UpdateAsync(int id, Usuario user, CancellationToken ct = default);
        Task<bool> DeleteAsync(int id, CancellationToken ct = default);
    }
}
