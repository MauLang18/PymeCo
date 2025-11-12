namespace POS.Application.DTOs;

public class OrderTotalsDto
{
    public decimal Subtotal { get; set; }
    public decimal Impuestos { get; set; }
    public decimal Total { get; set; }
}
