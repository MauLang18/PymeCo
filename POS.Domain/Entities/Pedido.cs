using System;
using System.Collections.Generic;

namespace POS.Domain.Entities
{
    public class Pedido
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        public string UsuarioId { get; set; } = string.Empty;
        public DateTime Fecha { get; set; } = DateTime.Now;

        public decimal Subtotal { get; set; }
        public decimal Impuestos { get; set; }
        public decimal Total { get; set; }

        public string Estado { get; set; } = "Pendiente";

        // Navegación
        public Client? Cliente { get; set; }
        public ApplicationUser? Usuario { get; set; }
        public ICollection<PedidoDetalle> Detalles { get; set; } = new List<PedidoDetalle>();
    }
}