using Microsoft.AspNetCore.Identity;

namespace POS.Domain.Entities;


public class ApplicationUser : IdentityUser
{
    // Propiedades adicionales personalizadas
    public string FullName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
}