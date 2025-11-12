using Microsoft.EntityFrameworkCore;
using POS.Domain.Entities;
using POS.Infrastructure.Persistence;

namespace POS.Infrastructure.Repositories;

public class PedidoRepository : IPedidoRepository
{
    private readonly AppDbContext _db;

    public PedidoRepository(AppDbContext db) => _db = db;

    public Task AddAsync(Pedido entity, CancellationToken ct = default) =>
        _db.Pedidos.AddAsync(entity, ct).AsTask();

    public void Update(Pedido entity) => _db.Pedidos.Update(entity);

    public void Remove(Pedido entity) => _db.Pedidos.Remove(entity);

    public async Task<Pedido?> GetByIdAsync(int id, bool includeDetails = false, CancellationToken ct = default)
    {
        var query = _db.Pedidos.AsQueryable();

        if (includeDetails)
        {
            query = query
                .Include(p => p.Cliente)
                .Include(p => p.Usuario)
                .Include(p => p.Detalles)
                    .ThenInclude(d => d.Producto);
        }

        return await query.FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task<IReadOnlyList<Pedido>> ListAsync(string? estado = null, CancellationToken ct = default)
    {
        var q = _db.Pedidos
            .Include(p => p.Cliente)
            .Include(p => p.Usuario)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(estado))
            q = q.Where(p => p.Estado == estado);

        return await q.OrderByDescending(p => p.Fecha).ToListAsync(ct);
    }

    public Task AddDetalleAsync(PedidoDetalle detalle, CancellationToken ct = default) =>
        _db.PedidoDetalles.AddAsync(detalle, ct).AsTask();

    public Task<PedidoDetalle?> GetDetalleByIdAsync(int detalleId, CancellationToken ct = default) =>
        _db.PedidoDetalles.FirstOrDefaultAsync(d => d.Id == detalleId, ct);

    public void UpdateDetalle(PedidoDetalle detalle) => _db.PedidoDetalles.Update(detalle);

    public void RemoveDetalle(PedidoDetalle detalle) => _db.PedidoDetalles.Remove(detalle);

    public Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        _db.SaveChangesAsync(ct);
}
