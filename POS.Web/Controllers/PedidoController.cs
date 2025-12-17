using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using POS.Application.DTOs;
using POS.Application.Interfaces;
using POS.Domain.Entities;
using POS.Infrastructure.GenerateExcel;
using POS.Infrastructure.GeneratePdf;

namespace POS.Web.Controllers
{
    public class PedidoController : Controller
    {
        private readonly IPedidoService _pedidoService;
        private readonly IClientService _clientService;
        private readonly IProductService _productService;
        private readonly IGenerateExcelService _generateExcelService;
        private readonly IGeneratePdfService _generatePdfService;
        private readonly ICompositeViewEngine _viewEngine;

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<PedidoController> _logger;

        public PedidoController(
            IPedidoService pedidoService,
            IClientService clientService,
            IProductService productService,
            IGenerateExcelService generateExcelService,
            IGeneratePdfService generatePdfService,
            ICompositeViewEngine viewEngine,
            UserManager<ApplicationUser> userManager,
            ILogger<PedidoController> logger
        )
        {
            _pedidoService = pedidoService;
            _clientService = clientService;
            _productService = productService;
            _generateExcelService = generateExcelService;
            _generatePdfService = generatePdfService;
            _viewEngine = viewEngine;

            _userManager = userManager;
            _logger = logger;
        }

        private async Task CargarCombosAsync(int? clienteSelected = null)
        {
            var clientes = await _clientService.ListAsync();
            var productos = await _productService.ListAsync();

            ViewBag.Clientes = new SelectList(clientes, "Id", "Name", clienteSelected);
            ViewBag.Productos = new SelectList(productos, "Id", "Name");
            ViewBag.ProductosData = productos;
        }

        private string GetUsuarioIdActual()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return userId ?? throw new UnauthorizedAccessException("Usuario no autenticado");
        }

        private async Task<string> GetUsuarioNombreActualAsync()
        {
            var identityUser = await _userManager.GetUserAsync(User);
            return identityUser?.FullName ?? identityUser?.UserName ?? "Usuario";
        }

        private static PedidoDto MapToDto(Pedido p)
        {
            return new PedidoDto
            {
                Id = p.Id,
                ClienteId = p.ClienteId,
                ClienteNombre = p.Cliente?.Name,
                UsuarioId = p.UsuarioId,
                UsuarioNombre = p.Usuario?.FullName ?? p.Usuario?.UserName,
                Fecha = p.Fecha,
                Subtotal = p.Subtotal,
                Impuestos = p.Impuestos,
                Total = p.Total,
                EstadoPedido = p.Estado,
                Detalles =
                    p.Detalles?.OrderBy(d => d.Id)
                        .Select(d => new PedidoDetalleDto
                        {
                            Id = d.Id,
                            ProductoId = d.ProductoId,
                            ProductoNombre = d.Producto?.Name,
                            Cantidad = d.Cantidad,
                            PrecioUnitario = d.PrecioUnit,
                            DescuentoPorc = d.Descuento,
                            ImpuestoPorc = d.ImpuestoPorc,
                            TotalLinea = d.TotalLinea,
                        })
                        .ToList() ?? new(),
            };
        }

        private void LogModelStateErrors()
        {
            if (ModelState.IsValid)
                return;
            foreach (var kv in ModelState)
            {
                var key = kv.Key;
                var state = kv.Value;
                foreach (var error in state.Errors)
                {
                    _logger.LogWarning(
                        "ModelState error in {Key}: {Error}",
                        key,
                        error.ErrorMessage
                    );
                }
            }
        }

        // ==========================================
        // Helper: Renderizar una vista Razor a string
        // ==========================================
        private async Task<string> RenderViewToStringAsync(string viewName, object model)
        {
            ViewData.Model = model;

            using var sw = new StringWriter();
            var viewResult = _viewEngine.FindView(ControllerContext, viewName, isMainPage: false);

            if (!viewResult.Success)
                throw new InvalidOperationException(
                    $"No se encontró la vista '{viewName}' para renderizar PDF."
                );

            var viewContext = new ViewContext(
                ControllerContext,
                viewResult.View,
                ViewData,
                TempData,
                sw,
                new HtmlHelperOptions()
            );

            await viewResult.View.RenderAsync(viewContext);
            return sw.ToString();
        }

        // ==========================
        // LISTADO
        // ==========================
        public async Task<IActionResult> Index(string estado, CancellationToken ct)
        {
            var filtro = string.IsNullOrWhiteSpace(estado) ? null : estado;
            var entities = await _pedidoService.ListAsync(filtro, ct);
            var listDto = entities.Select(MapToDto).ToList();
            return View("ListPedido", listDto);
        }

        // ==========================
        // EXCEL (GENERAL LISTADO)
        // ==========================
        [HttpGet]
        public async Task<IActionResult> ExportExcel(string estado, CancellationToken ct)
        {
            var filtro = string.IsNullOrWhiteSpace(estado) ? null : estado;
            var entities = await _pedidoService.ListAsync(filtro, ct);
            var listDto = entities.Select(MapToDto).ToList();

            var columns = new List<(string ColumnName, string PropertyName)>
            {
                ("#", "Id"),
                ("Fecha", "Fecha"),
                ("Cliente", "ClienteNombre"),
                ("Usuario", "UsuarioNombre"),
                ("Subtotal", "Subtotal"),
                ("Impuestos", "Impuestos"),
                ("Total", "Total"),
                ("Estado", "EstadoPedido"),
            };

            var bytes = _generateExcelService.GenerateExcel(listDto, columns);

            return File(
                bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Pedidos_{DateTime.Now:yyyyMMdd_HHmm}.xlsx"
            );
        }

        // ==========================
        // DETALLE
        // ==========================
        [HttpGet]
        public async Task<IActionResult> DetailsPedido(int id, CancellationToken ct)
        {
            var entity = await _pedidoService.GetByIdAsync(id, ct);
            if (entity == null)
                return NotFound();
            return View("DetailsPedido", MapToDto(entity));
        }

        // ==========================
        // PDF FACTURA (SOLO DETALLE)
        // ==========================
        [HttpGet]
        public async Task<IActionResult> ExportPdf(int id, CancellationToken ct)
        {
            var entity = await _pedidoService.GetByIdAsync(id, ct);
            if (entity == null)
                return NotFound();

            var dto = MapToDto(entity);

            // Vista dedicada para PDF (sin layout)
            var html = await RenderViewToStringAsync("FacturaPedidoPdf", dto);

            // Generador genérico (no BD)
            var pdfBytes = _generatePdfService.GeneratePdf(html);

            return File(pdfBytes, "application/pdf", $"Pedido_{id}.pdf");
        }

        // ==========================
        // CREATE
        // ==========================
        [HttpGet]
        public async Task<IActionResult> CreatePedido()
        {
            await CargarCombosAsync();

            var usuarioId = GetUsuarioIdActual();
            var usuarioNombre = await GetUsuarioNombreActualAsync();

            var dto = new PedidoDto { UsuarioId = usuarioId, EstadoPedido = "Pendiente" };

            ViewBag.UsuarioNombre = usuarioNombre;

            return View("CreatePedido", dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePedido(PedidoDto dto, CancellationToken ct)
        {
            dto.UsuarioId = GetUsuarioIdActual();
            if (string.IsNullOrWhiteSpace(dto.EstadoPedido))
                dto.EstadoPedido = "Pendiente";

            if (dto.Detalles == null || dto.Detalles.Count == 0)
                ModelState.AddModelError("", "Debes agregar al menos un detalle.");
            else if (dto.Detalles.Any(d => d.ProductoId <= 0 || d.Cantidad <= 0))
                ModelState.AddModelError("", "Cada detalle debe tener Producto y Cantidad > 0.");

            if (!ModelState.IsValid)
            {
                LogModelStateErrors();
                await CargarCombosAsync(dto.ClienteId);
                ViewBag.UsuarioNombre = await GetUsuarioNombreActualAsync();
                return View("CreatePedido", dto);
            }

            try
            {
                var id = await _pedidoService.CreateAsync(dto, ct);
                return RedirectToAction(nameof(DetailsPedido), new { id });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(
                    ex,
                    "DbUpdateException guardando Pedido. Inner: {Inner}",
                    ex.InnerException?.Message
                );
                ModelState.AddModelError(
                    "",
                    "No se pudo guardar el pedido. Verifica que Cliente, Usuario y Productos existan en la base de datos."
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando pedido");
                ModelState.AddModelError("", "Ocurrió un error guardando el pedido.");
            }

            await CargarCombosAsync(dto.ClienteId);
            ViewBag.UsuarioNombre = await GetUsuarioNombreActualAsync();
            return View("CreatePedido", dto);
        }

        // ==========================
        // EDIT
        // ==========================
        [HttpGet]
        public async Task<IActionResult> EditPedido(int id, CancellationToken ct)
        {
            var entity = await _pedidoService.GetByIdAsync(id, ct);
            if (entity == null)
                return NotFound();
            var dto = MapToDto(entity);

            await CargarCombosAsync(dto.ClienteId);
            return View("EditPedido", dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPedido(int id, PedidoDto dto, CancellationToken ct)
        {
            if (id != dto.Id)
                return BadRequest();
            dto.UsuarioId = GetUsuarioIdActual();
            if (string.IsNullOrWhiteSpace(dto.EstadoPedido))
                dto.EstadoPedido = "Pendiente";

            if (dto.Detalles == null || dto.Detalles.Count == 0)
                ModelState.AddModelError("", "Debes agregar al menos un detalle.");
            else if (dto.Detalles.Any(d => d.ProductoId <= 0 || d.Cantidad <= 0))
                ModelState.AddModelError("", "Cada detalle debe tener Producto y Cantidad > 0.");

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
            catch (DbUpdateException ex)
            {
                _logger.LogError(
                    ex,
                    "DbUpdateException actualizando Pedido {Id}. Inner: {Inner}",
                    id,
                    ex.InnerException?.Message
                );
                ModelState.AddModelError(
                    "",
                    "No se pudo actualizar el pedido. Verifica que Cliente, Usuario y Productos existan."
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando pedido {Id}", id);
                ModelState.AddModelError("", "Ocurrió un error actualizando el pedido.");
            }

            await CargarCombosAsync(dto.ClienteId);
            return View("EditPedido", dto);
        }

        // ==========================
        // DELETE
        // ==========================
        [HttpGet]
        public async Task<IActionResult> DeletePedido(int id, CancellationToken ct)
        {
            var entity = await _pedidoService.GetByIdAsync(id, ct);
            if (entity == null)
                return NotFound();
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
            catch (DbUpdateException ex)
            {
                _logger.LogError(
                    ex,
                    "DbUpdateException eliminando pedido {Id}. Inner: {Inner}",
                    id,
                    ex.InnerException?.Message
                );
                TempData["Msg"] =
                    "No se pudo eliminar el pedido. Puede tener relaciones dependientes.";
                return RedirectToAction(nameof(DetailsPedido), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error eliminando pedido {Id}", id);
                TempData["Msg"] = "Ocurrió un error al eliminar el pedido.";
                return RedirectToAction(nameof(DetailsPedido), new { id });
            }
        }
    }
}
