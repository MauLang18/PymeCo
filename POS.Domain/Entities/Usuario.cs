namespace POS.Domain.Entities;

public class Usuario
{
    public int Id { get; set; }
    public string Nombre { get; set; } = null!;
    public string EstadoUsuario { get; set; } = "Activo";

    public int? RolId { get; set; }
    public Rol? Rol { get; set; }
}
