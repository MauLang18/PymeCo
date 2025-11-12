using Microsoft.AspNetCore.Mvc;
using POS.Application.DTOs;
using POS.Application.Interfaces;
using POS.Domain.Enums;

namespace POS.Web.Controllers.Api;

[ApiController]
[Route("api")]
[Produces("application/json")]
public class APIController : ControllerBase
{
    private readonly IProductService _products;

    public APIController(IProductService products) => _products = products;

    [HttpGet("productos/buscar")]
    public async Task<ActionResult<IEnumerable<ProductSearchResultDto>>> BuscarProductos(
    [FromQuery] string? q,
    CancellationToken ct)
    {
        // El repositorio ya devuelve: Activos + Stock>0 + TOP 10 + Ordenados
        var items = await _products.ListAsync(q, ct);

        var result = items.Select(p => new ProductSearchResultDto
        {
            Id = p.Id,
            Name = p.Name,
            Price = p.Price,
            TaxPercent = p.TaxPercent,
            Stock = p.Stock
        }).ToList();

        return Ok(result);
    }

    // POST /api/pedidos/calcular
    // Body: [{ productId, cantidad, descuento }]
    [HttpPost("pedidos/calcular")]
    public async Task<ActionResult<OrderTotalsDto>> CalcularTotales(
        [FromBody] List<OrderCalcLineDto> lineas,
        CancellationToken ct)
    {
        if (lineas is null || lineas.Count == 0)
            return BadRequest("Debe enviar al menos una línea.");

        decimal subtotal = 0m, impuestos = 0m;

        foreach (var l in lineas)
        {
            if (l.Cantidad <= 0) return BadRequest("Cantidad debe ser > 0.");
            if (l.Descuento < 0 || l.Descuento > 100)
                return BadRequest("Descuento debe estar entre 0 y 100.");

            var prod = await _products.GetByIdAsync(l.ProductId, ct);
            if (prod is null) return NotFound($"Producto {l.ProductId} no existe.");

            var lineaBruta = prod.Price * l.Cantidad;
            var montoDesc = lineaBruta * (l.Descuento / 100m);
            var baseImponible = lineaBruta - montoDesc;
            var montoImpuesto = baseImponible * (prod.TaxPercent / 100m);

            subtotal += Math.Round(baseImponible, 2, MidpointRounding.AwayFromZero);
            impuestos += Math.Round(montoImpuesto, 2, MidpointRounding.AwayFromZero);
        }

        var total = subtotal + impuestos;
        return Ok(new OrderTotalsDto
        {
            Subtotal = Math.Round(subtotal, 2, MidpointRounding.AwayFromZero),
            Impuestos = Math.Round(impuestos, 2, MidpointRounding.AwayFromZero),
            Total = Math.Round(total, 2, MidpointRounding.AwayFromZero)
        });
    }
}
