using System.Linq;
using POS.Application.DTOs;
using POS.Application.Interfaces;
using POS.Domain.Entities;
using POS.Infrastructure.Repositories;

namespace POS.Application.Services;

public class PedidoService : IPedidoService
{
    private readonly IPedidoRepository _repo;

    public PedidoService(IPedidoRepository repo) => _repo = repo;

    public async Task<int> CreateAsync(PedidoDto dto, CancellationToken ct = default)
    {
        var entity = new Pedido
        {
            ClienteId = dto.ClienteId,
            UsuarioId = dto.UsuarioId,
            Fecha = dto.Fecha,
            Estado = dto.EstadoPedido,
        };

        if (dto.Detalles != null)
        {
            foreach (var d in dto.Detalles)
            {
                entity.Detalles.Add(new PedidoDetalle
                {
                    ProductoId = d.ProductoId,
                    Cantidad = d.Cantidad,
                    PrecioUnit = d.PrecioUnitario,
                    Descuento = d.DescuentoPorc,
                    ImpuestoPorc = d.ImpuestoPorc,
                    TotalLinea = d.TotalLinea
                });
            }
        }

        Recalcular(entity);

        await _repo.AddAsync(entity, ct);
        await _repo.SaveChangesAsync(ct);

        return entity.Id;
    }

    public async Task UpdateAsync(int id, PedidoDto dto, CancellationToken ct = default)
    {
        var entity = await _repo.GetByIdAsync(id, includeDetails: true, ct)
                     ?? throw new KeyNotFoundException("Pedido no encontrado.");

        entity.ClienteId = dto.ClienteId;
        entity.UsuarioId = dto.UsuarioId;
        entity.Estado = dto.EstadoPedido;

        _repo.Update(entity);
        Recalcular(entity);

        await _repo.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await _repo.GetByIdAsync(id, includeDetails: false, ct)
                     ?? throw new KeyNotFoundException("Pedido no encontrado.");

        _repo.Remove(entity);
        await _repo.SaveChangesAsync(ct);
    }

    public Task<Pedido?> GetByIdAsync(int id, CancellationToken ct = default) =>
        _repo.GetByIdAsync(id, includeDetails: true, ct);

    public Task<IReadOnlyList<Pedido>> ListAsync(string? estado = null, CancellationToken ct = default) =>
        _repo.ListAsync(estado, ct);

    public async Task<int> AddDetalleAsync(int pedidoId, PedidoDetalleDto d, CancellationToken ct = default)
    {
        var pedido = await _repo.GetByIdAsync(pedidoId, includeDetails: true, ct)
                     ?? throw new KeyNotFoundException("Pedido no encontrado.");

        var detalle = new PedidoDetalle
        {
            PedidoId = pedidoId,
            ProductoId = d.ProductoId,
            Cantidad = d.Cantidad,
            PrecioUnit = d.PrecioUnitario,
            Descuento = d.DescuentoPorc,
            ImpuestoPorc = d.ImpuestoPorc,
            TotalLinea = d.TotalLinea
        };

        await _repo.AddDetalleAsync(detalle, ct);
        await RecalcularTotalesAsync(pedidoId, ct);

        return detalle.Id;
    }

    public async Task UpdateDetalleAsync(int detalleId, PedidoDetalleDto d, CancellationToken ct = default)
    {
        var det = await _repo.GetDetalleByIdAsync(detalleId, ct)
                  ?? throw new KeyNotFoundException("Detalle no encontrado.");

        det.ProductoId = d.ProductoId;
        det.Cantidad = d.Cantidad;
        det.PrecioUnit = d.PrecioUnitario;
        det.Descuento = d.DescuentoPorc;
        det.ImpuestoPorc = d.ImpuestoPorc;
        det.TotalLinea = d.TotalLinea;

        _repo.UpdateDetalle(det);
        await _repo.SaveChangesAsync(ct);

        await RecalcularTotalesAsync(det.PedidoId, ct);
    }

    public async Task RemoveDetalleAsync(int detalleId, CancellationToken ct = default)
    {
        var det = await _repo.GetDetalleByIdAsync(detalleId, ct)
                  ?? throw new KeyNotFoundException("Detalle no encontrado.");

        var pedidoId = det.PedidoId;

        _repo.RemoveDetalle(det);
        await _repo.SaveChangesAsync(ct);

        await RecalcularTotalesAsync(pedidoId, ct);
    }

    public async Task RecalcularTotalesAsync(int pedidoId, CancellationToken ct = default)
    {
        var pedido = await _repo.GetByIdAsync(pedidoId, includeDetails: true, ct)
                     ?? throw new KeyNotFoundException("Pedido no encontrado.");

        Recalcular(pedido);
        _repo.Update(pedido);
        await _repo.SaveChangesAsync(ct);
    }

    public async Task CambiarEstadoAsync(int pedidoId, string nuevoEstado, CancellationToken ct = default)
    {
        var pedido = await _repo.GetByIdAsync(pedidoId, includeDetails: false, ct)
                     ?? throw new KeyNotFoundException("Pedido no encontrado.");

        pedido.Estado = nuevoEstado;

        _repo.Update(pedido);
        await _repo.SaveChangesAsync(ct);
    }

    private static void Recalcular(Pedido p)
    {
        if (p.Detalles == null || p.Detalles.Count == 0)
        {
            p.Subtotal = 0;
            p.Impuestos = 0;
            p.Total = 0;
            return;
        }

        foreach (var d in p.Detalles)
        {
            if (d.TotalLinea <= 0)
            {
                var bruto = d.PrecioUnit * d.Cantidad;
                var descMonto = bruto * (d.Descuento / 100m);
                var baseLinea = bruto - descMonto;
                var imp = baseLinea * (d.ImpuestoPorc / 100m);
                d.TotalLinea = decimal.Round(baseLinea + imp, 2);
            }
        }

        var subtotal = p.Detalles.Sum(d =>
        {
            var bruto = d.PrecioUnit * d.Cantidad;
            var descMonto = bruto * (d.Descuento / 100m);
            return bruto - descMonto;
        });

        var impuestos = p.Detalles.Sum(d =>
        {
            var bruto = d.PrecioUnit * d.Cantidad;
            var descMonto = bruto * (d.Descuento / 100m);
            var baseLinea = bruto - descMonto;
            return baseLinea * (d.ImpuestoPorc / 100m);
        });

        p.Subtotal = decimal.Round(subtotal, 2);
        p.Impuestos = decimal.Round(impuestos, 2);
        p.Total = decimal.Round(p.Subtotal + p.Impuestos, 2);
    }
}
