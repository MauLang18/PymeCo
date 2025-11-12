using POS.Application.DTOs;
using POS.Application.Interfaces;
using POS.Domain.Entities;
using POS.Infrastructure.Repositories;

namespace POS.Application.Services
{
    public class PedidoService : IPedidoService
    {
        private readonly IPedidoRepository _repo;

        public PedidoService(IPedidoRepository repo)
        {
            _repo = repo;
        }

        // -------------------- PEDIDO (HEADER) --------------------

        public async Task<int> CreateAsync(PedidoDto dto, CancellationToken ct = default)
        {
            var entity = new Pedido
            {
                ClienteId = dto.ClienteId,
                UsuarioId = dto.UsuarioId,
                Fecha = DateTime.UtcNow,
                Estado = string.IsNullOrWhiteSpace(dto.EstadoPedido) ? "Pendiente" : dto.EstadoPedido
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
                        TotalLinea = 0m 
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
            var entity = await _repo.GetByIdAsync(id, includeDetails: true, ct: ct);
            if (entity == null) throw new KeyNotFoundException($"Pedido {id} no encontrado");

            entity.ClienteId = dto.ClienteId;
            entity.UsuarioId = dto.UsuarioId;
            entity.Estado = string.IsNullOrWhiteSpace(dto.EstadoPedido) ? entity.Estado : dto.EstadoPedido;

            entity.Detalles.Clear();
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
                        TotalLinea = 0m
                    });
                }
            }

            Recalcular(entity);

            _repo.Update(entity);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            var entity = await _repo.GetByIdAsync(id, includeDetails: false, ct: ct);
            if (entity == null) return;

            _repo.Remove(entity);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task<Pedido?> GetByIdAsync(int id, CancellationToken ct = default)
            => await _repo.GetByIdAsync(id, includeDetails: true, ct: ct);

        public async Task<IReadOnlyList<Pedido>> ListAsync(string? estado = null, CancellationToken ct = default)
        {
            return await _repo.ListAsync(estado, ct);
        }

        // -------------------- DETALLES --------------------

        public async Task<int> AddDetalleAsync(int pedidoId, PedidoDetalleDto detalle, CancellationToken ct = default)
        {
            var pedido = await _repo.GetByIdAsync(pedidoId, includeDetails: true, ct: ct);
            if (pedido == null) throw new KeyNotFoundException($"Pedido {pedidoId} no encontrado");

            var det = new PedidoDetalle
            {
                PedidoId = pedidoId,
                ProductoId = detalle.ProductoId,
                Cantidad = detalle.Cantidad,
                PrecioUnit = detalle.PrecioUnitario,
                Descuento = detalle.DescuentoPorc,
                ImpuestoPorc = detalle.ImpuestoPorc,
                TotalLinea = 0m
            };

            pedido.Detalles.Add(det);
            Recalcular(pedido);

            _repo.Update(pedido);
            await _repo.SaveChangesAsync(ct);

            return det.Id;
        }

        public async Task UpdateDetalleAsync(int detalleId, PedidoDetalleDto detalle, CancellationToken ct = default)
        {
            var det = await _repo.GetDetalleByIdAsync(detalleId, ct);
            if (det == null) throw new KeyNotFoundException($"Detalle {detalleId} no encontrado");

            det.ProductoId = detalle.ProductoId;
            det.Cantidad = detalle.Cantidad;
            det.PrecioUnit = detalle.PrecioUnitario;
            det.Descuento = detalle.DescuentoPorc;
            det.ImpuestoPorc = detalle.ImpuestoPorc;
            det.TotalLinea = 0m;

            _repo.UpdateDetalle(det);

            var pedido = await _repo.GetByIdAsync(det.PedidoId, includeDetails: true, ct: ct);
            if (pedido != null)
            {
                Recalcular(pedido);
                _repo.Update(pedido);
            }

            await _repo.SaveChangesAsync(ct);
        }

        public async Task RemoveDetalleAsync(int detalleId, CancellationToken ct = default)
        {
            var det = await _repo.GetDetalleByIdAsync(detalleId, ct);
            if (det == null) return;

            var pedidoId = det.PedidoId;

            _repo.RemoveDetalle(det);

            var pedido = await _repo.GetByIdAsync(pedidoId, includeDetails: true, ct: ct);
            if (pedido != null)
            {
                var match = pedido.Detalles.FirstOrDefault(x => x.Id == detalleId);
                if (match != null) pedido.Detalles.Remove(match);

                Recalcular(pedido);
                _repo.Update(pedido);
            }

            await _repo.SaveChangesAsync(ct);
        }

        // -------------------- UTILIDADES --------------------

        public async Task RecalcularTotalesAsync(int pedidoId, CancellationToken ct = default)
        {
            var pedido = await _repo.GetByIdAsync(pedidoId, includeDetails: true, ct: ct);
            if (pedido == null) throw new KeyNotFoundException($"Pedido {pedidoId} no encontrado");

            Recalcular(pedido);
            _repo.Update(pedido);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task CambiarEstadoAsync(int pedidoId, string nuevoEstado, CancellationToken ct = default)
        {
            var pedido = await _repo.GetByIdAsync(pedidoId, includeDetails: false, ct: ct);
            if (pedido == null) throw new KeyNotFoundException($"Pedido {pedidoId} no encontrado");

            if (!string.IsNullOrWhiteSpace(nuevoEstado))
                pedido.Estado = nuevoEstado.Trim();

            _repo.Update(pedido);
            await _repo.SaveChangesAsync(ct);
        }

        // -------------------- Cálculo de totales --------------------

        private static void Recalcular(Pedido p)
        {
            decimal subtotal = 0m, impuestos = 0m, total = 0m;

            foreach (var d in p.Detalles)
            {
                var bruto = d.Cantidad * d.PrecioUnit;
                var conDesc = bruto * (1 - (d.Descuento / 100m));
                var totLn = conDesc * (1 + (d.ImpuestoPorc / 100m));
                var impMon = totLn - conDesc;

                d.TotalLinea = Math.Round(totLn, 2, MidpointRounding.AwayFromZero);

                subtotal += (totLn - impMon);
                impuestos += impMon;
                total += totLn;
            }

            p.Subtotal = Math.Round(subtotal, 2, MidpointRounding.AwayFromZero);
            p.Impuestos = Math.Round(impuestos, 2, MidpointRounding.AwayFromZero);
            p.Total = Math.Round(total, 2, MidpointRounding.AwayFromZero);
        }
    }
}
