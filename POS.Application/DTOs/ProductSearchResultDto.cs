namespace POS.Application.DTOs;

public class ProductSearchResultDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public decimal Price { get; set; }
    public decimal TaxPercent { get; set; }
    public int Stock { get; set; }
}
