using POS.Application.DTOs;
using POS.Domain.Entities;

namespace POS.Application.Interfaces;

public interface IPedidoService
{
    Task<int> CreateAsync(PedidoDto dto, CancellationToken ct = default);
    Task UpdateAsync(int id, PedidoDto dto, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);

    Task<Pedido?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<Pedido>> ListAsync(string? estado = null, CancellationToken ct = default);

    Task<int> AddDetalleAsync(int pedidoId, PedidoDetalleDto detalle, CancellationToken ct = default);
    Task UpdateDetalleAsync(int detalleId, PedidoDetalleDto detalle, CancellationToken ct = default);

    Task RemoveDetalleAsync(int detalleId, CancellationToken ct = default);
    Task RecalcularTotalesAsync(int pedidoId, CancellationToken ct = default);
    Task CambiarEstadoAsync(int pedidoId, string nuevoEstado, CancellationToken ct = default);
}
