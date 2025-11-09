using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using POS.Application.DTOs;
using POS.Application.Interfaces;

namespace POS.Web.Controllers;

public class ClientController : Controller
{
    private readonly IClientService _service;
    private readonly ILogger<ClientController> _logger;

    public ClientController(IClientService service, ILogger<ClientController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> ListClient(string? q, CancellationToken ct)
    {
        _logger.LogInformation("GET ListClient started. Query={Query}", q);
        var items = await _service.ListAsync(q, ct);
        var count = items?.Count() ?? 0;
        _logger.LogInformation("GET ListClient completed. Returned {Count} items", count);
        return View("ListClient", items);
    }

    [HttpGet]
    public IActionResult CreateClient()
    {
        _logger.LogInformation("GET CreateClient view requested");
        return View("CreateClient", new ClientDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateClient(ClientDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning(
                "POST CreateClient invalid model. Errors={ErrorCount}",
                ModelState.ErrorCount
            );
            return View("CreateClient", dto);
        }

        _logger.LogInformation("POST CreateClient started");
        var id = await _service.CreateAsync(dto, ct);
        _logger.LogInformation("POST CreateClient succeeded. Created Id={Id}", id);
        return RedirectToAction(nameof(DetailsClient), new { id });
    }

    [HttpGet]
    public async Task<IActionResult> EditClient(int id, CancellationToken ct)
    {
        _logger.LogInformation("GET EditClient started. Id={Id}", id);
        var item = await _service.GetByIdAsync(id, ct);
        if (item is null)
        {
            _logger.LogWarning("GET EditClient NotFound. Id={Id}", id);
            return NotFound();
        }

        _logger.LogInformation("GET EditClient loaded. Id={Id}", id);
        return View("EditClient", item);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditClient(int id, ClientDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning(
                "POST EditClient invalid model. Id={Id} Errors={ErrorCount}",
                id,
                ModelState.ErrorCount
            );
            return View("EditClient", dto);
        }

        _logger.LogInformation("POST EditClient started. Id={Id}", id);
        await _service.UpdateAsync(id, dto, ct);
        _logger.LogInformation("POST EditClient succeeded. Id={Id}", id);
        return RedirectToAction(nameof(DetailsClient), new { id });
    }

    [HttpGet]
    public async Task<IActionResult> DeleteClient(int id, CancellationToken ct)
    {
        _logger.LogInformation("GET DeleteClient confirmation started. Id={Id}", id);
        var item = await _service.GetByIdAsync(id, ct);
        if (item is null)
        {
            _logger.LogWarning("GET DeleteClient NotFound. Id={Id}", id);
            return NotFound();
        }

        _logger.LogInformation("GET DeleteClient confirmation loaded. Id={Id}", id);
        return View("DeleteClient", item);
    }

    [HttpPost, ActionName("DeleteClient")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteClientConfirmed(int id, CancellationToken ct)
    {
        _logger.LogInformation("POST DeleteClientConfirmed started. Id={Id}", id);
        await _service.DeleteAsync(id, ct);
        _logger.LogInformation("POST DeleteClientConfirmed succeeded. Id={Id}", id);
        return RedirectToAction(nameof(ListClient));
    }

    [HttpGet]
    public async Task<IActionResult> DetailsClient(int id, CancellationToken ct)
    {
        _logger.LogInformation("GET DetailsClient started. Id={Id}", id);
        var item = await _service.GetByIdAsync(id, ct);
        if (item is null)
        {
            _logger.LogWarning("GET DetailsClient NotFound. Id={Id}", id);
            return NotFound();
        }

        _logger.LogInformation("GET DetailsClient loaded. Id={Id}", id);
        return View("DetailsClient", item);
    }
}
