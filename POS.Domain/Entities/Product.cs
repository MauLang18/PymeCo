using POS.Domain.Enums;

namespace POS.Domain.Entities;

public class Product
{
  public int Id { get; set; }

  public string Name { get; set; } = null!;
  public int CategoryId { get; set; }

  public decimal Price { get; set; }
  public decimal TaxPercent { get; set; }

  public int Stock { get; set; }
  public string? ImageUrl { get; set; }

  public ProductStatus Status { get; set; } = ProductStatus.Active;

  public DateTime CreatedAt { get; set; }
  public DateTime? UpdatedAt { get; set; }
}
