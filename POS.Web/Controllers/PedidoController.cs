using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using POS.Application.DTOs;
using POS.Application.Interfaces;
using POS.Domain.Entities;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace POS.Web.Controllers
{
    public class PedidoController : Controller
    {
        private readonly IPedidoService _pedidoService;
        private readonly IClientService _clientService;
        private readonly IProductService _productService;
        private readonly ILogger<PedidoController> _logger;

        public PedidoController(
            IPedidoService pedidoService,
            IClientService clientService,
            IProductService productService,
            ILogger<PedidoController> logger)
        {
            _pedidoService = pedidoService;
            _clientService = clientService;
            _productService = productService;
            _logger = logger;
        }

        // ---------- Helpers ----------
        private async Task CargarCombosAsync(int? clienteSelected = null)
        {
            var clientes = await _clientService.ListAsync();
            var productos = await _productService.ListAsync();

            ViewBag.Clientes = new SelectList(clientes, "Id", "Name", clienteSelected);
            ViewBag.Productos = new SelectList(productos, "Id", "Name");
        }

        private int GetUsuarioIdActual()
        {
            var claim = User.FindFirstValue("UsuarioId") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(claim, out var id) ? id : 1;
        }

        private static PedidoDto MapToDto(Pedido p)
        {
            return new PedidoDto
            {
                Id = p.Id,
                ClienteId = p.ClienteId,
                ClienteNombre = p.Cliente?.Name,
                UsuarioId = p.UsuarioId,
                UsuarioNombre = p.Usuario?.Nombre,
                Fecha = p.Fecha,
                Subtotal = p.Subtotal,
                Impuestos = p.Impuestos,
                Total = p.Total,
                EstadoPedido = p.Estado,
                Detalles = p.Detalles?.OrderBy(d => d.Id).Select(d => new PedidoDetalleDto
                {
                    Id = d.Id,
                    ProductoId = d.ProductoId,
                    ProductoNombre = d.Producto?.Name,
                    Cantidad = d.Cantidad,
                    PrecioUnitario = d.PrecioUnit,
                    DescuentoPorc = d.Descuento,
                    ImpuestoPorc = d.ImpuestoPorc,
                    TotalLinea = d.TotalLinea
                }).ToList() ?? new()
            };
        }

        private void LogModelStateErrors()
        {
            if (ModelState.IsValid) return;
            foreach (var kv in ModelState)
            {
                var key = kv.Key;
                var state = kv.Value;
                foreach (var error in state.Errors)
                {
                    _logger.LogWarning("ModelState error in {Key}: {Error}", key, error.ErrorMessage);
                }
            }
        }

        // ---------- List ----------
        public async Task<IActionResult> Index(string estado, CancellationToken ct)
        {
            var filtro = string.IsNullOrWhiteSpace(estado) ? null : estado;
            var entities = await _pedidoService.ListAsync(filtro, ct);
            var listDto = entities.Select(MapToDto).ToList();
            return View("ListPedido", listDto);
        }

        // ---------- Details ----------
        [HttpGet]
        public async Task<IActionResult> DetailsPedido(int id, CancellationToken ct)
        {
            var entity = await _pedidoService.GetByIdAsync(id, ct);
            if (entity == null) return NotFound();
            return View("DetailsPedido", MapToDto(entity));
        }

        // ---------- Create ----------
        [HttpGet]
        public async Task<IActionResult> CreatePedido()
        {
            await CargarCombosAsync();
            var dto = new PedidoDto
            {
                UsuarioId = GetUsuarioIdActual(),
                EstadoPedido = "Pendiente"
            };
            return View("CreatePedido", dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePedido(PedidoDto dto, CancellationToken ct)
        {
            dto.UsuarioId = GetUsuarioIdActual();
            if (string.IsNullOrWhiteSpace(dto.EstadoPedido))
                dto.EstadoPedido = "Pendiente";

            if (!ModelState.IsValid)
            {
                LogModelStateErrors();
                await CargarCombosAsync(dto.ClienteId);
                return View("CreatePedido", dto);
            }

            try
            {
                var id = await _pedidoService.CreateAsync(dto, ct);
                return RedirectToAction(nameof(DetailsPedido), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando pedido");
                ModelState.AddModelError("", "Ocurrió un error guardando el pedido.");
                await CargarCombosAsync(dto.ClienteId);
                return View("CreatePedido", dto);
            }
        }

        // ---------- Edit ----------
        [HttpGet]
        public async Task<IActionResult> EditPedido(int id, CancellationToken ct)
        {
            var entity = await _pedidoService.GetByIdAsync(id, ct);
            if (entity == null) return NotFound();
            var dto = MapToDto(entity);

            await CargarCombosAsync(dto.ClienteId);
            return View("EditPedido", dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPedido(int id, PedidoDto dto, CancellationToken ct)
        {
            if (id != dto.Id) return BadRequest();
            dto.UsuarioId = GetUsuarioIdActual();
            if (string.IsNullOrWhiteSpace(dto.EstadoPedido))
                dto.EstadoPedido = "Pendiente";

            if (!ModelState.IsValid)
            {
                LogModelStateErrors();
                await CargarCombosAsync(dto.ClienteId);
                return View("EditPedido", dto);
            }

            try
            {
                await _pedidoService.UpdateAsync(id, dto, ct);
                return RedirectToAction(nameof(DetailsPedido), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando pedido {Id}", id);
                ModelState.AddModelError("", "Ocurrió un error actualizando el pedido.");
                await CargarCombosAsync(dto.ClienteId);
                return View("EditPedido", dto);
            }
        }

        // ---------- Delete ----------
        [HttpGet]
        public async Task<IActionResult> DeletePedido(int id, CancellationToken ct)
        {
            var entity = await _pedidoService.GetByIdAsync(id, ct);
            if (entity == null) return NotFound();
            return View("DeletePedido", MapToDto(entity));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken ct)
        {
            try
            {
                await _pedidoService.DeleteAsync(id, ct);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error eliminando pedido {Id}", id);
                return RedirectToAction(nameof(DetailsPedido), new { id });
            }
        }
    }
}
