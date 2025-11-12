namespace POS.Application.DTOs;

public class OrderCalcLineDto
{
    public int ProductId { get; set; }
    public int Cantidad { get; set; }
    public decimal Descuento { get; set; }
}
