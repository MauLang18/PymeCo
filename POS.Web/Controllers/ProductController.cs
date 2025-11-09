using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using POS.Application.DTOs;
using POS.Application.Interfaces;
using POS.Domain.Enums;

namespace POS.Web.Controllers;

public class ProductController : Controller
{
    private readonly IProductService _service;
    private readonly ILogger<ProductController> _logger;

    public ProductController(IProductService service, ILogger<ProductController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public async Task<IActionResult> ListProduct(string? q, CancellationToken ct)
    {
        _logger.LogInformation("GET ListProduct started. Query={Query}", q);
        var items = await _service.ListAsync(q, ct);
        var count = items?.Count() ?? 0;
        _logger.LogInformation("GET ListProduct completed. Returned {Count} items", count);
        return View("ListProduct", items);
    }

    [HttpGet]
    public IActionResult CreateProduct()
    {
        _logger.LogInformation("GET CreateProduct view requested");
        return View("CreateProduct", new ProductDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateProduct(ProductDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning(
                "POST CreateProduct invalid model. Errors={ErrorCount}",
                ModelState.ErrorCount
            );
            return View("CreateProduct", dto);
        }

        _logger.LogInformation("POST CreateProduct started");
        var id = await _service.CreateAsync(dto, ct);
        _logger.LogInformation("POST CreateProduct succeeded. Created Id={Id}", id);
        return RedirectToAction(nameof(DetailsProduct), new { id });
    }

    [HttpGet]
    public async Task<IActionResult> EditProduct(int id, CancellationToken ct)
    {
        _logger.LogInformation("GET EditProduct started. Id={Id}", id);
        var entity = await _service.GetByIdAsync(id, ct);
        if (entity is null)
        {
            _logger.LogWarning("GET EditProduct NotFound. Id={Id}", id);
            return NotFound();
        }

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
        _logger.LogInformation("GET EditProduct loaded. Id={Id}", id);
        return View("EditProduct", dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditProduct(int id, ProductDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning(
                "POST EditProduct invalid model. Id={Id} Errors={ErrorCount}",
                id,
                ModelState.ErrorCount
            );
            ViewBag.ProductId = id;
            return View("EditProduct", dto);
        }

        _logger.LogInformation("POST EditProduct started. Id={Id}", id);
        await _service.UpdateAsync(id, dto, ct);
        _logger.LogInformation("POST EditProduct succeeded. Id={Id}", id);
        return RedirectToAction(nameof(DetailsProduct), new { id });
    }

    [HttpGet]
    public async Task<IActionResult> DeleteProduct(int id, CancellationToken ct)
    {
        _logger.LogInformation("GET DeleteProduct confirmation started. Id={Id}", id);
        var entity = await _service.GetByIdAsync(id, ct);
        if (entity is null)
        {
            _logger.LogWarning("GET DeleteProduct NotFound. Id={Id}", id);
            return NotFound();
        }

        _logger.LogInformation("GET DeleteProduct confirmation loaded. Id={Id}", id);
        return View("DeleteProduct", entity);
    }

    [HttpPost, ActionName("DeleteProduct")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteProductConfirmed(int id, CancellationToken ct)
    {
        _logger.LogInformation("POST DeleteProductConfirmed started. Id={Id}", id);
        await _service.DeleteAsync(id, ct);
        _logger.LogInformation("POST DeleteProductConfirmed succeeded. Id={Id}", id);
        return RedirectToAction(nameof(ListProduct));
    }

    [HttpGet]
    public async Task<IActionResult> DetailsProduct(int id, CancellationToken ct)
    {
        _logger.LogInformation("GET DetailsProduct started. Id={Id}", id);
        var entity = await _service.GetByIdAsync(id, ct);
        if (entity is null)
        {
            _logger.LogWarning("GET DetailsProduct NotFound. Id={Id}", id);
            return NotFound();
        }

        _logger.LogInformation("GET DetailsProduct loaded. Id={Id}", id);
        return View("DetailsProduct", entity);
    }
}
