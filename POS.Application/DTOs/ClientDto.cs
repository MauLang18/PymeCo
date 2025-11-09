using System.ComponentModel.DataAnnotations;

namespace POS.Application.DTOs;

public class ClientDto
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = null!;

    [MaxLength(40)]
    public string? NationalId { get; set; }

    [EmailAddress, MaxLength(200)]
    public string? Email { get; set; }

    [Phone, MaxLength(50)]
    public string? Phone { get; set; }

    [MaxLength(500)]
    public string? Address { get; set; }
}
