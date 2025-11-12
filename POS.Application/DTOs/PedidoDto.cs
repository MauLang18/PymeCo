using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace POS.Application.DTOs;

public class PedidoDto
{
    public int? Id { get; set; }

    [Required, Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un cliente válido.")]
    public int ClienteId { get; set; }
    public string? ClienteNombre { get; set; }

    [Required, Range(1, int.MaxValue, ErrorMessage = "Debe indicar un usuario válido.")]
    public int UsuarioId { get; set; }
    public string? UsuarioNombre { get; set; }

    public DateTime Fecha { get; set; } = DateTime.Now;

    [Range(0, double.MaxValue)]
    public decimal Subtotal { get; set; }

    [Range(0, double.MaxValue)]
    public decimal Impuestos { get; set; }

    [Range(0, double.MaxValue)]
    public decimal Total { get; set; }

    [Required, MaxLength(50)]
    public string EstadoPedido { get; set; } = "Pendiente"; // Pendiente, Pagado, Enviado, Cancelado

    [MinLength(1, ErrorMessage = "Debe agregar al menos un detalle al pedido.")]
    public List<PedidoDetalleDto> Detalles { get; set; } = new();
}
