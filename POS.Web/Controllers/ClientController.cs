using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.Application.DTOs;
using POS.Application.Interfaces;
using POS.Infrastructure.GenerateExcel;

namespace POS.Web.Controllers;

/// <summary>
/// Controlador de clientes con autorización por roles
/// - Admin: Puede hacer todo
/// - Vendedor: Puede ver, crear y editar (no eliminar)
/// - Cajero: Solo puede ver
/// </summary>
[Authorize]
public class ClientController : Controller
{
    private readonly IClientService _service;
    private readonly ILogger<ClientController> _logger;
    private readonly IGenerateExcelService _generateExcelService;

    public ClientController(
        IClientService service,
        ILogger<ClientController> logger,
        IGenerateExcelService generateExcelService
    )
    {
        _service = service;
        _logger = logger;
        _generateExcelService = generateExcelService;
    }

    // ==========================
    // LISTADO
    // ==========================
    [HttpGet]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [Authorize(Roles = "Admin,Vendedor,Cajero")]
    public async Task<IActionResult> ListClient(string? q, CancellationToken ct)
    {
        _logger.LogInformation("GET ListClient started. Query={Query}", q);
        var items = await _service.ListAsync(q, ct);
        _logger.LogInformation(
            "GET ListClient completed. Returned {Count} items",
            items?.Count() ?? 0
        );
        return View("ListClient", items);
    }

    // ==========================
    // EXPORT EXCEL (LISTADO)
    // ==========================
    [HttpGet]
    [Authorize(Roles = "Admin,Vendedor,Cajero")]
    public async Task<IActionResult> ExportExcel(string? q, CancellationToken ct)
    {
        var items = await _service.ListAsync(q, ct);

        var columns = new List<(string ColumnName, string PropertyName)>
        {
            ("ID", "Id"),
            ("Nombre", "Name"),
            ("Cédula", "NationalId"),
            ("Correo", "Email"),
            ("Teléfono", "Phone"),
            ("Dirección", "Address"),
        };

        var bytes = _generateExcelService.GenerateExcel(items, columns);

        return File(
            bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"Clientes_{DateTime.Now:yyyyMMdd_HHmm}.xlsx"
        );
    }

    // ==========================
    // CREATE
    // ==========================
    [HttpGet]
    [Authorize(Roles = "Admin,Vendedor")]
    public IActionResult CreateClient()
    {
        return View("CreateClient", new ClientDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Vendedor")]
    public async Task<IActionResult> CreateClient(ClientDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return View("CreateClient", dto);

        var id = await _service.CreateAsync(dto, ct);
        return RedirectToAction(nameof(DetailsClient), new { id });
    }

    // ==========================
    // EDIT
    // ==========================
    [HttpGet]
    [Authorize(Roles = "Admin,Vendedor")]
    public async Task<IActionResult> EditClient(int id, CancellationToken ct)
    {
        var item = await _service.GetByIdAsync(id, ct);
        if (item is null)
            return NotFound();
        return View("EditClient", item);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Vendedor")]
    public async Task<IActionResult> EditClient(int id, ClientDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return View("EditClient", dto);

        await _service.UpdateAsync(id, dto, ct);
        return RedirectToAction(nameof(DetailsClient), new { id });
    }

    // ==========================
    // DELETE
    // ==========================
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteClient(int id, CancellationToken ct)
    {
        var item = await _service.GetByIdAsync(id, ct);
        if (item is null)
            return NotFound();
        return View("DeleteClient", item);
    }

    [HttpPost, ActionName("DeleteClient")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteClientConfirmed(int id, CancellationToken ct)
    {
        await _service.DeleteAsync(id, ct);
        return RedirectToAction(nameof(ListClient));
    }

    // ==========================
    // DETAILS
    // ==========================
    [HttpGet]
    [Authorize(Roles = "Admin,Vendedor,Cajero")]
    public async Task<IActionResult> DetailsClient(int id, CancellationToken ct)
    {
        var item = await _service.GetByIdAsync(id, ct);
        if (item is null)
            return NotFound();
        return View("DetailsClient", item);
    }
}

