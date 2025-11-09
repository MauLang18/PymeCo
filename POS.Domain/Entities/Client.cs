namespace POS.Domain.Entities;

public class Client
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;
        public string? NationalId { get; set; }      // Cedula
        public string? Email { get; set; }           // Correo
        public string? Phone { get; set; }           // Telefono
        public string? Address { get; set; }         // Direccion

        public DateTime CreatedAt { get; set; }      // FechaRegistro
        public DateTime? UpdatedAt { get; set; }     // FechaModificacion
    }
