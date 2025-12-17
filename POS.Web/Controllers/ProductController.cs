using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.Application.DTOs;
using POS.Application.Interfaces;
using POS.Domain.Entities;
using POS.Domain.Enums;
using POS.Infrastructure.FileStorage;
using POS.Infrastructure.GenerateExcel;

namespace POS.Web.Controllers;

/// <summary>
/// Controlador de productos con autorización por roles
/// - Admin: Puede hacer todo
/// - Vendedor: Ver listado y detalles
/// - Cajero: Ver listado y detalles
/// </summary>
[Authorize]
public class ProductController : Controller
{
    private readonly IProductService _service;
    private readonly ILogger<ProductController> _logger;
    private readonly IFileStorageLocal _files;
    private readonly IGenerateExcelService _generateExcelService;

    public ProductController(
        IProductService service,
        ILogger<ProductController> logger,
        IFileStorageLocal files,
        IGenerateExcelService generateExcelService
    )
    {
        _service = service;
        _logger = logger;
        _files = files;
        _generateExcelService = generateExcelService;
    }

    // ==========================
    // LISTADO
    // ==========================
    [HttpGet]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [Authorize(Roles = "Admin,Vendedor,Cajero")]
    public async Task<IActionResult> ListProduct(string? q, CancellationToken ct)
    {
        _logger.LogInformation("GET ListProduct started. Query={Query}", q);
        var items = await _service.ListAsync(q, ct);
        _logger.LogInformation(
            "GET ListProduct completed. Returned {Count} items",
            items?.Count() ?? 0
        );
        return View("ListProduct", items);
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
            ("CategoriaId", "CategoryId"),
            ("Precio", "Price"),
            ("IVA (%)", "TaxPercent"),
            ("Stock", "Stock"),
            ("Estado", "Status"),
        };

        var bytes = _generateExcelService.GenerateExcel(items, columns);

        return File(
            bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"Productos_{DateTime.Now:yyyyMMdd_HHmm}.xlsx"
        );
    }

    // ==========================
    // CREATE
    // ==========================
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public IActionResult CreateProduct()
    {
        return View("CreateProduct", new ProductDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateProduct(ProductDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return View("CreateProduct", dto);

        if (dto.ImageFile is { Length: > 0 })
            dto.ImageUrl = await _files.SaveAsync(dto.ImageFile, "products");

        var id = await _service.CreateAsync(dto, ct);
        return RedirectToAction(nameof(DetailsProduct), new { id });
    }

    // ==========================
    // EDIT
    // ==========================
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> EditProduct(int id, CancellationToken ct)
    {
        var entity = await _service.GetByIdAsync(id, ct);
        if (entity is null)
            return NotFound();

        var dto = new ProductDto
        {
            Name = entity.Name,
            CategoryId = entity.CategoryId,
            Price = entity.Price,
            TaxPercent = entity.TaxPercent,
            Stock = entity.Stock,
            ImageUrl = entity.ImageUrl,
            Active = entity.Status == ProductStatus.Active,
        };

        ViewBag.ProductId = id;
        return View("EditProduct", dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> EditProduct(int id, ProductDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.ProductId = id;
            return View("EditProduct", dto);
        }

        var current = await _service.GetByIdAsync(id, ct);
        if (current is null)
            return NotFound();

        if (dto.ImageFile is { Length: > 0 })
            dto.ImageUrl = await _files.SaveAsync(dto.ImageFile, "products");
        else
            dto.ImageUrl ??= current.ImageUrl;

        await _service.UpdateAsync(id, dto, ct);
        return RedirectToAction(nameof(DetailsProduct), new { id });
    }

    // ==========================
    // DELETE
    // ==========================
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteProduct(int id, CancellationToken ct)
    {
        var entity = await _service.GetByIdAsync(id, ct);
        if (entity is null)
            return NotFound();
        return View("DeleteProduct", entity);
    }

    [HttpPost, ActionName("DeleteProduct")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteProductConfirmed(int id, CancellationToken ct)
    {
        await _service.DeleteAsync(id, ct);
        return RedirectToAction(nameof(ListProduct));
    }

    // ==========================
    // DETAILS
    // ==========================
    [HttpGet]
    [Authorize(Roles = "Admin,Vendedor,Cajero")]
    public async Task<IActionResult> DetailsProduct(int id, CancellationToken ct)
    {
        var entity = await _service.GetByIdAsync(id, ct);
        if (entity is null)
            return NotFound();
        return View("DetailsProduct", entity);
    }
}

