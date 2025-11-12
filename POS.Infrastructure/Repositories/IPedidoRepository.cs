using POS.Domain.Entities;

namespace POS.Infrastructure.Repositories;

public interface IPedidoRepository
{
    // Pedido
    Task AddAsync(Pedido entity, CancellationToken ct = default);
    void Update(Pedido entity);
    void Remove(Pedido entity);
    Task<Pedido?> GetByIdAsync(int id, bool includeDetails = false, CancellationToken ct = default);
    Task<IReadOnlyList<Pedido>> ListAsync(string? estado = null, CancellationToken ct = default);

    // Detalle
    Task AddDetalleAsync(PedidoDetalle detalle, CancellationToken ct = default);
    Task<PedidoDetalle?> GetDetalleByIdAsync(int detalleId, CancellationToken ct = default);
    void UpdateDetalle(PedidoDetalle detalle);
    void RemoveDetalle(PedidoDetalle detalle);

    // Persistencia
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
