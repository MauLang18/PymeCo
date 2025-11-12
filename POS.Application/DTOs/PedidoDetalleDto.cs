using System.ComponentModel.DataAnnotations;

namespace POS.Application.DTOs;

public class PedidoDetalleDto
{
    public int? Id { get; set; }

    [Required, Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un producto válido.")]
    public int ProductoId { get; set; }

    public string? ProductoNombre { get; set; }

    [Required, Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0.")]
    public int Cantidad { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "El precio unitario no puede ser negativo.")]
    public decimal PrecioUnitario { get; set; }

    [Range(0, 100, ErrorMessage = "El descuento debe estar entre 0 y 100%.")]
    public decimal DescuentoPorc { get; set; }

    [Range(0, 100, ErrorMessage = "El impuesto debe estar entre 0 y 100%.")]
    public decimal ImpuestoPorc { get; set; }

    [Range(0, double.MaxValue)]
    public decimal TotalLinea { get; set; }
}
