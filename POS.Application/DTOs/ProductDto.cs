using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace POS.Application.DTOs;

public class ProductDto
{
    [Required, MaxLength(200)]
    public string Name { get; set; } = null!;

    [Range(1, int.MaxValue)]
    public int CategoryId { get; set; }

    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }

    [Range(0, 100)]
    public decimal TaxPercent { get; set; }

    [Range(0, int.MaxValue)]
    public int Stock { get; set; }

    public IFormFile? ImageFile { get; set; }

    [MaxLength(400)]
    public string? ImageUrl { get; set; }

    public bool Active { get; set; } = true; // convenience for UI
}
