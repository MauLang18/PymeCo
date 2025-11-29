using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using POS.Domain.Entities;
using POS.Infrastructure.Persistence; // change to your actual DbContext namespace

namespace POS.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _db;

        public UserRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<int> CreateAsync(Usuario user, CancellationToken ct = default)
        {
            _db.Set<Usuario>().Add(user);
            await _db.SaveChangesAsync(ct);
            return user.Id;
        }

        public async Task<Usuario?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _db.Set<Usuario>()
                .Include(u => u.Rol)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id, ct);
        }

        public async Task<IReadOnlyList<Usuario>> ListAsync(CancellationToken ct = default)
        {
            return await _db.Set<Usuario>()
                .Include(u => u.Rol)
                .AsNoTracking()
                .OrderBy(x => x.Nombre)
                .ToListAsync(ct);
        }

        public async Task<bool> UpdateAsync(Usuario user, CancellationToken ct = default)
        {
            _db.Set<Usuario>().Update(user);
            var changes = await _db.SaveChangesAsync(ct);
            return changes > 0;
        }

        public async Task<bool> DeleteAsync(Usuario user, CancellationToken ct = default)
        {
            _db.Set<Usuario>().Remove(user);
            var changes = await _db.SaveChangesAsync(ct);
            return changes > 0;
        }
    }
}
