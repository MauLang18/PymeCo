using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using POS.Application.DTOs;
using POS.Application.Interfaces;

namespace POS.Web.Controllers;

[AutoValidateAntiforgeryToken]
public class PedidoController : Controller
{
    private readonly IPedidoService _service;
    private readonly ILogger<PedidoController> _logger;

    public PedidoController(IPedidoService service, ILogger<PedidoController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public async Task<IActionResult> ListPedido(string? estado, CancellationToken ct)
    {
        _logger.LogInformation("GET ListPedido started. Estado={Estado}", estado);
        var items = await _service.ListAsync(estado, ct);
        var count = items?.Count ?? 0;
        _logger.LogInformation("GET ListPedido completed. Returned {Count} items", count);
        return View("ListPedido", items);
    }

    [HttpGet]
    public IActionResult CreatePedido()
    {
        _logger.LogInformation("GET CreatePedido view requested");
        return View("CreatePedido", new PedidoDto());
    }

    [HttpPost]
    public async Task<IActionResult> CreatePedido(PedidoDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("POST CreatePedido invalid model. Errors={Errors}", ModelState.ErrorCount);
            return View("CreatePedido", dto);
        }

        _logger.LogInformation("POST CreatePedido started");
        var id = await _service.CreateAsync(dto, ct);
        _logger.LogInformation("POST CreatePedido succeeded. Created Id={Id}", id);
        return RedirectToAction(nameof(DetailsPedido), new { id });
    }

    [HttpGet]
    public async Task<IActionResult> EditPedido(int id, CancellationToken ct)
    {
        _logger.LogInformation("GET EditPedido started. Id={Id}", id);
        var entity = await _service.GetByIdAsync(id, ct);
        if (entity is null)
        {
            _logger.LogWarning("GET EditPedido NotFound. Id={Id}", id);
            return NotFound();
        }

        // Map a DTO para el formulario principal (los detalles se editan con endpoints anidados)
        var dto = new PedidoDto
        {
            ClienteId = entity.ClienteId,
            UsuarioId = entity.UsuarioId,
            Fecha = entity.Fecha,
            Subtotal = entity.Subtotal,
            Impuestos = entity.Impuestos,
            Total = entity.Total,
            EstadoPedido = entity.Estado
        };

        ViewBag.PedidoId = id;
        _logger.LogInformation("GET EditPedido loaded. Id={Id}", id);
        return View("EditPedido", dto);
    }

    [HttpPost]
    public async Task<IActionResult> EditPedido(int id, PedidoDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("POST EditPedido invalid model. Id={Id} Errors={Errors}", id, ModelState.ErrorCount);
            ViewBag.PedidoId = id;
            return View("EditPedido", dto);
        }

        _logger.LogInformation("POST EditPedido started. Id={Id}", id);
        await _service.UpdateAsync(id, dto, ct);
        _logger.LogInformation("POST EditPedido succeeded. Id={Id}", id);
        return RedirectToAction(nameof(DetailsPedido), new { id });
    }

    [HttpGet]
    public async Task<IActionResult> DeletePedido(int id, CancellationToken ct)
    {
        _logger.LogInformation("GET DeletePedido confirmation. Id={Id}", id);
        var entity = await _service.GetByIdAsync(id, ct);
        if (entity is null)
        {
            _logger.LogWarning("GET DeletePedido NotFound. Id={Id}", id);
            return NotFound();
        }

        return View("DeletePedido", entity);
    }

    [HttpPost, ActionName("DeletePedido")]
    public async Task<IActionResult> DeletePedidoConfirmed(int id, CancellationToken ct)
    {
        _logger.LogInformation("POST DeletePedidoConfirmed started. Id={Id}", id);
        await _service.DeleteAsync(id, ct);
        _logger.LogInformation("POST DeletePedidoConfirmed succeeded. Id={Id}", id);
        return RedirectToAction(nameof(ListPedido));
    }

    [HttpGet]
    public async Task<IActionResult> DetailsPedido(int id, CancellationToken ct)
    {
        _logger.LogInformation("GET DetailsPedido started. Id={Id}", id);
        var entity = await _service.GetByIdAsync(id, ct);
        if (entity is null)
        {
            _logger.LogWarning("GET DetailsPedido NotFound. Id={Id}", id);
            return NotFound();
        }

        _logger.LogInformation("GET DetailsPedido loaded. Id={Id}", id);
        return View("DetailsPedido", entity);
    }

    // Agregar detalle a un pedido existente
    [HttpPost]
    public async Task<IActionResult> AddDetalle(int pedidoId, PedidoDetalleDto detalle, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("POST AddDetalle invalid model. PedidoId={PedidoId} Errors={Errors}",
                pedidoId, ModelState.ErrorCount);
            // Redirige a editar para mostrar validaciones
            return RedirectToAction(nameof(EditPedido), new { id = pedidoId });
        }

        _logger.LogInformation("POST AddDetalle started. PedidoId={PedidoId}", pedidoId);
        await _service.AddDetalleAsync(pedidoId, detalle, ct);
        _logger.LogInformation("POST AddDetalle succeeded. PedidoId={PedidoId}", pedidoId);

        return RedirectToAction(nameof(EditPedido), new { id = pedidoId });
    }

    // Actualizar un detalle específico
    [HttpPost]
    public async Task<IActionResult> UpdateDetalle(int detalleId, PedidoDetalleDto detalle, int pedidoId, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("POST UpdateDetalle invalid model. DetalleId={DetalleId} Errors={Errors}",
                detalleId, ModelState.ErrorCount);
            return RedirectToAction(nameof(EditPedido), new { id = pedidoId });
        }

        _logger.LogInformation("POST UpdateDetalle started. DetalleId={DetalleId}", detalleId);
        await _service.UpdateDetalleAsync(detalleId, detalle, ct);
        _logger.LogInformation("POST UpdateDetalle succeeded. DetalleId={DetalleId}", detalleId);

        return RedirectToAction(nameof(EditPedido), new { id = pedidoId });
    }

    // Eliminar un detalle
    [HttpPost]
    public async Task<IActionResult> RemoveDetalle(int detalleId, int pedidoId, CancellationToken ct)
    {
        _logger.LogInformation("POST RemoveDetalle started. DetalleId={DetalleId}", detalleId);
        await _service.RemoveDetalleAsync(detalleId, ct);
        _logger.LogInformation("POST RemoveDetalle succeeded. DetalleId={DetalleId}", detalleId);
        return RedirectToAction(nameof(EditPedido), new { id = pedidoId });
    }

    // Recalcular totales del pedido
    [HttpPost]
    public async Task<IActionResult> Recalcular(int pedidoId, CancellationToken ct)
    {
        _logger.LogInformation("POST Recalcular started. PedidoId={PedidoId}", pedidoId);
        await _service.RecalcularTotalesAsync(pedidoId, ct);
        _logger.LogInformation("POST Recalcular completed. PedidoId={PedidoId}", pedidoId);
        return RedirectToAction(nameof(EditPedido), new { id = pedidoId });
    }

    // Cambiar estado del pedido (Pendiente|Pagado|Enviado|Cancelado)
    [HttpPost]
    public async Task<IActionResult> CambiarEstado(int pedidoId, string estado, CancellationToken ct)
    {
        _logger.LogInformation("POST CambiarEstado started. PedidoId={PedidoId} Estado={Estado}", pedidoId, estado);
        await _service.CambiarEstadoAsync(pedidoId, estado, ct);
        _logger.LogInformation("POST CambiarEstado succeeded. PedidoId={PedidoId} Estado={Estado}", pedidoId, estado);
        return RedirectToAction(nameof(EditPedido), new { id = pedidoId });
    }
}
